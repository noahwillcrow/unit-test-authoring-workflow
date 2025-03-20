package main

import (
	"context"
	"flag"

	"github.com/openai/openai-go"
	"github.com/openai/openai-go/option"
)

func main() {
	inputArgs := parseInputArgs()

	println(inputArgs.fileName, inputArgs.apiKey, inputArgs.baseURL, inputArgs.modelName)

	openaiClient := openai.NewClient(
		option.WithBaseURL(inputArgs.baseURL),
		option.WithAPIKey(inputArgs.apiKey),
	)

	chatCompletion, err := openaiClient.Chat.Completions.New(context.TODO(), openai.ChatCompletionNewParams{
		Messages: openai.F([]openai.ChatCompletionMessageParamUnion{
			openai.UserMessage(inputArgs.fileName),
		}),
		Model: openai.F(inputArgs.modelName),
	})
	if err != nil {
		panic(err.Error())
	}

	println(chatCompletion.Choices[0].Message.Content)
}

type inputArgsStruct struct {
	fileName string

	apiKey    string
	baseURL   string
	modelName string
}

func parseInputArgs() *inputArgsStruct {

	var ret inputArgsStruct = inputArgsStruct{}

	flag.StringVar(&ret.fileName, "filename", "", "The filename to read")

	flag.StringVar(&ret.apiKey, "apikey", "", "The API key to use")
	flag.StringVar(&ret.baseURL, "baseurl", "", "The base URL to use")
	flag.StringVar(&ret.modelName, "modelname", "", "The model name to use")

	flag.Parse()

	return &ret
}
