package internal

import (
	"log"
	"os"

	"gopkg.in/yaml.v2"
)

// Config is a struct that holds the configuration for the application
type Config struct {
	apiKey    string
	baseURL   string
	modelName string
}

// LoadConfig creates a Config by loading it from the given file path
func LoadConfig(configYamlFilePath string) *Config {
	file, err := os.Open(configYamlFilePath)
	if err != nil {
		log.Fatalf("Failed to open config file: %v", err)
	}
	defer file.Close()

	var config Config
	decoder := yaml.NewDecoder(file)
	if err := decoder.Decode(&config); err != nil {
		log.Fatalf("Failed to decode config file: %v", err)
	}

	return &config
}

// GetAPIKey gets the API key on the Config
func (config *Config) GetAPIKey() string {
	return config.apiKey
}

// GetBaseURL gets the base URL on the Config
func (config *Config) GetBaseURL() string {
	return config.baseURL
}

// GetModelName gets the model name on the Config
func (config *Config) GetModelName() string {
	return config.modelName
}
