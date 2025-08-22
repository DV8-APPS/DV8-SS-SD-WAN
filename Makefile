dev: ## run controller + mock devices
	docker compose -f dev/docker-compose.yaml up -d
proto:
	buf generate
openapi:
	npx @redocly/cli build-docs openapi.yaml -o dist/api.html
e2e:
	npx playwright test
