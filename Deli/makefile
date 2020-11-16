CONTENTS     = $(addsuffix .dll,$(NAME) Atlas DotNetZip)
ZIP          = $(NAME).zip
TEMP         = temp
TEMP_PLUGINS = $(TEMP)/BepInEx/plugins/Deli

.PHONY: all build pack clean

all: clean build pack

build:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

pack: build
	mkdir -p $(TEMP_PLUGINS)
	
	mv $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS)) $(TEMP_PLUGINS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -rf $(TEMP)
	rm -f $(ZIP)