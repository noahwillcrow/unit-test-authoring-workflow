package main

import (
	"context"
	"flag"

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

	chatCompletion, err := openaiClient.Chat.Completions.New(context.TODO(), openai.ChatCompletionNewParams{
		Messages: openai.F([]openai.ChatCompletionMessageParamUnion{
			openai.UserMessage(inputArgs.filePath),
		}),
		Model: openai.F(config.GetModelName()),
	})
	if err != nil {
		panic(err.Error())
	}

	println(chatCompletion.Choices[0].Message.Content)
}

type inputArgsStruct struct {
	configYamlFilePath string
	filePath           string
}

func parseInputArgs() *inputArgsStruct {
	var ret inputArgsStruct = inputArgsStruct{}

	flag.StringVar(&ret.configYamlFilePath, "config", "", "The path to the config to read and use")
	flag.StringVar(&ret.filePath, "file", "", "The path to the file to write unit tests against")

	flag.Parse()

	return &ret
}
