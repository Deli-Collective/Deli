CONTENTS = manifest.json $(NAME).dll
ZIP      = $(NAME).deli

.PHONY: all build clean

all: clean build $(ZIP)

build:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

$(ZIP): build
	zip -9j $@ $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS))

clean:
	rm -f $(ZIP)
