package internal

import (
	"log"
	"os"
	"path/filepath"

	"gopkg.in/yaml.v2"
)

type innerConfig struct {
	APIKey    string `yaml:"apiKey"`
	BaseURL   string `yaml:"baseURL"`
	ModelName string `yaml:"modelName"`
}

// Config is a struct that holds the configuration for the application
type Config struct {
	innerConfig innerConfig
}

// LoadConfig creates a Config by loading it from the given file path
func LoadConfig(configYamlFilePath string) *Config {
	filename, _ := filepath.Abs(configYamlFilePath)
	yamlFile, err := os.ReadFile(filename)
	if err != nil {
		log.Fatalf("Failed to open config file: %v", err)
	}

	var innerConfig innerConfig
	if err := yaml.Unmarshal(yamlFile, &innerConfig); err != nil {
		panic(err)
	}

	var config Config
	config.innerConfig = innerConfig

	println("Config loaded successfully")

	return &config
}

// GetAPIKey gets the API key on the Config
func (config *Config) GetAPIKey() string {
	return config.innerConfig.APIKey
}

// GetBaseURL gets the base URL on the Config
func (config *Config) GetBaseURL() string {
	return config.innerConfig.BaseURL
}

// GetModelName gets the model name on the Config
func (config *Config) GetModelName() string {
	return config.innerConfig.ModelName
}
