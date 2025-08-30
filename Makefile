dev: ## run controller + mock devices
	docker compose -f dev/docker-compose.yaml up -d
proto:
	buf generate
openapi:
	npx @redocly/cli build-docs openapi.yaml -o dist/api.html
e2e:
	npx playwright test

db-build: ## create SQL Server schema
	sqlcmd -S SS-VEGA-DEV\\SSVGASTD -U ssqs_admin -P SSDv8P@QL2025 -i db/mssql_build.sql
