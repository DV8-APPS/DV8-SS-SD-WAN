dev: ## run controller + mock devices
	docker compose -f dev/docker-compose.yaml up -d

proto:
	buf generate

openapi:
	npx @redocly/cli build-docs openapi.yaml -o dist/api.html

e2e:
	npx playwright test

# New commands for comprehensive testing and setup
setup: ## One-click setup: install dependencies, initialize DB, configure
	python setup.py

test: ## Run all tests
	python -m pytest tests/ -v

test-integration: ## Run comprehensive integration tests
	python test_comprehensive_integration.py

run: ## Start the DV8 SD-WAN application
	python run_dv8.py

run-dev: ## Start with hot reload for development
	uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

clean: ## Clean database and temporary files
	rm -f dv8.db
	rm -rf __pycache__ app/__pycache__ tests/__pycache__
	rm -rf .pytest_cache

health: ## Check system health and dependencies
	python -c "from app.main import check_dependencies, preload_dotnet_sdk; check_dependencies(); preload_dotnet_sdk(); print('✓ All dependencies OK')"

install: setup test ## Complete install: setup + test

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

.PHONY: dev proto openapi e2e setup test test-integration run run-dev clean health install help
.DEFAULT_GOAL := help
