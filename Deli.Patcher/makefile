include ../makefile.var
CONSTANTS_SOURCE = src/Common/Constants.cs
NUPKG            = $(NUGET_DIR)/$(NAME).$(VERSION).nupkg

.PHONY: build macros-pre compile macros-post

$(NUPKG): build
	mkdir -p $(dir $@)
	dotnet pack --configuration $(CONFIG) $(BUILD_PROPERTIES) --no-build --output $(dir $@)

build: compile macros-post

macros-pre:
	tee $(CONSTANTS_SOURCE).old < $(CONSTANTS_SOURCE)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		-e 's|MACRO_GIT_DESCRIBE|$(GIT_DESCRIBE)|g' \
		-e 's|MACRO_GIT_BRANCH|$(GIT_BRANCH)|g' \
		-e 's|MACRO_GIT_HASH|$(GIT_HASH)|g' \
		$(CONSTANTS_SOURCE)

compile: macros-pre
	$(BUILD)

macros-post: compile
	mv $(CONSTANTS_SOURCE).old $(CONSTANTS_SOURCE)
