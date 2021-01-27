include ../makefile.var
NUPKG = $(NUGET_DIR)/$(NAME).$(VERSION).nupkg

.PHONY: build compile

$(NUPKG): build
	mkdir -p $(dir $@)
	dotnet pack --configuration $(CONFIG) $(BUILD_PROPERTIES) --no-build --output $(dir $@)

build: compile

compile:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)
