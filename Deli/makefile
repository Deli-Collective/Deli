CONSTANTS_SOURCE = src/DeliConstants.cs
NUPKG            = $(NUGET_DIR)/$(NAME).$(VERSION).nupkg

.PHONY: build macros-precompile compile macros-postcompile

$(NUPKG): build
	mkdir -p $(dir $@)
	dotnet pack --configuration $(CONFIG) $(BUILD_PROPERTIES) --no-build --output $(dir $@)

build: compile macros-postcompile

macros-precompile:
	tee $(CONSTANTS_SOURCE).old < $(CONSTANTS_SOURCE)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		-e 's|MACRO_GIT_DESCRIBE|$(GIT_DESCRIBE)|g' \
		-e 's|MACRO_GIT_BRANCH|$(GIT_BRANCH)|g' \
		-e 's|MACRO_GIT_HASH|$(GIT_HASH)|g' \
		$(CONSTANTS_SOURCE)

compile: macros-precompile
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

macros-postcompile: compile
	mv $(CONSTANTS_SOURCE).old $(CONSTANTS_SOURCE)
