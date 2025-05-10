package main

import (
	"context"
	"flag"
	"log"
	"os"
	"os/exec"
	"strings"

	"github.com/noahwillcrow/unit-test-authoring-workflow/unit-test-authoring-workflow/internal"
	"github.com/openai/openai-go"
	"github.com/openai/openai-go/option"
)

func main() {
	inputArgs := parseInputArgs()

	config := internal.LoadConfig(inputArgs.configYamlFilePath)

	openaiClient := openai.NewClient(
		option.WithBaseURL(config.GetBaseURL()),
		option.WithAPIKey(config.GetAPIKey()),
	)

	println("Reading example test file contents...")
	exampleTestFileContents, err := os.ReadFile(inputArgs.exampleTestFilePath)
	if err != nil {
		log.Fatalf("Failed to read file: %v", err)
	}

	println("Reading file contents...")
	fileContents, err := os.ReadFile(inputArgs.filePath)
	if err != nil {
		log.Fatalf("Failed to read file: %v", err)
	}

	println("Loading C# dependencies explainer...")
	csharpDepsExplained, err := loadCsharpDepsExplainer(inputArgs.csproj, inputArgs.classFullName)
	if err != nil {
		log.Fatalf("Failed to load C# dependencies explainer: %v", err)
	}

	println("Preparing prompt...")
	prompt, err := loadPrompt(
		"./unit-test-authoring-workflow/prompt-templates/csharp-prompt.md",
		[][2]string{
			{"AdditionalInstructions", config.GetInstructions()},
			{"ClassToTest", string(fileContents)},
			{"DepsExplained", csharpDepsExplained},
			{"ExampleTestsFile", string(exampleTestFileContents)},
		})

	println("Prompt:")
	println(prompt)

	println("Waiting for LLM response...")
	chatCompletion, err := openaiClient.Chat.Completions.New(context.TODO(), openai.ChatCompletionNewParams{
		Messages: openai.F([]openai.ChatCompletionMessageParamUnion{
			openai.UserMessage(prompt),
		}),
		Model: openai.F(config.GetModelName()),
	})
	if err != nil {
		panic(err.Error())
	}

	println(chatCompletion.Choices[0].Message.Content)
}

func loadCsharpDepsExplainer(csprojFilePath string, classFullName string) (string, error) {
	cmd := exec.Command("dotnet", "run", "--project", "./llm-tools/roslyn-dep-explainer/roslyn-dep-explainer.csproj", "--", csprojFilePath, classFullName)
	out, err := cmd.CombinedOutput()
	if err != nil {
		return "", err
	}

	return string(out), nil
}

func loadPrompt(templateFilePath string, templatePairs [][2]string) (string, error) {
	templateFileContents, err := os.ReadFile(templateFilePath)
	if err != nil {
		return "", err
	}

	prompt := string(templateFileContents)

	for _, templatePair := range templatePairs {
		prompt = strings.ReplaceAll(prompt, "{{"+templatePair[0]+"}}", templatePair[1])
	}

	return prompt, nil
}

type inputArgsStruct struct {
	classFullName       string
	configYamlFilePath  string
	csproj              string
	exampleTestFilePath string
	filePath            string
}

func parseInputArgs() *inputArgsStruct {
	var ret inputArgsStruct = inputArgsStruct{}

	flag.StringVar(&ret.configYamlFilePath, "config", "", "The path to the config to read and use")
	flag.StringVar(&ret.classFullName, "class", "", "The full name of the target class to write tests against")
	flag.StringVar(&ret.csproj, "csproj", "", "The path to the csproj the target class lives in")
	flag.StringVar(&ret.exampleTestFilePath, "example", "", "The path to the example test files class that already exists")
	flag.StringVar(&ret.filePath, "file", "", "The file path of the target class to write tests against")

	flag.Parse()

	return &ret
}
