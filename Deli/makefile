CONSTANTS_SOURCE = Constants.cs
CONTENTS         = $(addsuffix .dll,$(NAME) Atlas DotNetZip)
ZIP              = $(NAME).zip
TEMP             = temp
TEMP_PLUGINS     = $(TEMP)/BepInEx/plugins/Deli

.PHONY: all macros-precompile build macros-postcompile pack clean

all: clean build pack

build: compile macros-postcompile

macros-precompile:
	tee $(CONSTANTS_SOURCE).old < $(CONSTANTS_SOURCE)
	sed -i \
		-e 's|STUB_GIT_DESCRIBE|$(GIT_DESCRIBE)|g' \
		-e 's|STUB_GIT_BRANCH|$(GIT_BRANCH)|g' \
		-e 's|STUB_GIT_HASH|$(GIT_HASH)|g' \
		$(CONSTANTS_SOURCE)

compile: macros-precompile
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

macros-postcompile: compile
	mv $(CONSTANTS_SOURCE).old $(CONSTANTS_SOURCE)

pack: build
	mkdir -p $(TEMP_PLUGINS)
	
	mv $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS)) $(TEMP_PLUGINS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)