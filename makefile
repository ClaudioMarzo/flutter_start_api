STARTUP_FOLDER=./FlutterStart.Apresentation
INFRA_FOLDER=./FlutterStart.Infrastructure
# TEST_FOLDER=./src/FlutterStart.Tests
SOLUTION=FlutterStartAPI.sln

.PHONY: clean restore build test run migrate remove_migration update_migration up_dev_db up_dev_api build_docker run_docker restart_docker stop_docker

clean:
	dotnet clean $(SOLUTION)
	find . -type d -name 'bin' -exec rm -rf {} +
	find . -type d -name 'obj' -exec rm -rf {} +

restore:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION)

test:
	dotnet test $(TEST_FOLDER)

run:
	dotnet run --project $(STARTUP_FOLDER)

migrate:
	dotnet ef migrations add $(name) --startup-project $(STARTUP_FOLDER) --project $(INFRA_FOLDER)

remove_migration:
	dotnet ef migrations remove --startup-project $(STARTUP_FOLDER) --project $(INFRA_FOLDER)

update_migration:
	dotnet ef database update --startup-project $(STARTUP_FOLDER) --project $(INFRA_FOLDER)

revert_last_migration:
	dotnet ef database update $(name) --startup-project $(STARTUP_FOLDER) --project $(INFRA_FOLDER)

up_dev_db:
	docker compose up -d flutter_start_database

up_dev_api:
	docker compose -f docker-compose.yml up -d flutter_start_api

# criar imagem do dockerfile
# docker build -t flutterstart_api .
# criar container a partir da imagem
# docker run --name flutterstart_api -p 3000:3000 flutterstart_api