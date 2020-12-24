CONTENTS += licenses.txt MonoMod.exe $(NAME).dll

.PHONY: all macros-pre compile macros-post clean

all: clean $(PACKAGE) macros-post

macros-pre:
	tee $(MANIFEST).old < $(MANIFEST)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		$(MANIFEST)

compile: macros-pre
	$(BUILD)

$(PACKAGE): compile
	zip -9j $@ $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS))

macros-post: $(PACKAGE)
	mv $(MANIFEST_OLD) $(MANIFEST)

clean:
	rm -f $(PACKAGE)
