include ../makefile.var
NUPKG = $(NUGET_DIR)/$(NAME).$(VERSION).nupkg

.PHONY: pack build compile

pack: $(NUPKG)
build: compile

$(NUPKG):
	mkdir -p $(dir $@)
	dotnet pack --configuration $(CONFIG) $(BUILD_PROPERTIES) --no-build --output $(dir $@)
	
compile:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)
