include ../makefile.var
CONSTANTS_SOURCE = src/Bootstrap/Constants.cs
NUPKG            = $(NUGET_DIR)/$(NAME).$(VERSION).nupkg

.PHONY: pack build precompile compile postcompile

pack: $(NUPKG)
build: precompile compile postcompile

$(NUPKG):
	mkdir -p $(dir $@)
	dotnet pack --configuration $(CONFIG) $(BUILD_PROPERTIES) --no-build --output $(dir $@)

precompile:
	tee $(CONSTANTS_SOURCE).old < $(CONSTANTS_SOURCE)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		-e 's|MACRO_SYS_VERSION|$(SYS_VERSION)|g' \
		-e 's|MACRO_GIT_DESCRIBE|$(GIT_DESCRIBE)|g' \
		-e 's|MACRO_GIT_BRANCH|$(GIT_BRANCH)|g' \
		-e 's|MACRO_GIT_HASH|$(GIT_HASH)|g' \
		$(CONSTANTS_SOURCE)

compile:
	$(BUILD)

postcompile:
	mv $(CONSTANTS_SOURCE).old $(CONSTANTS_SOURCE)
