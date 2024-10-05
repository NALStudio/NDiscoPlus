# Hue Icons source: https://github.com/arallsopp/hass-hue-icons
# Icons are for personal use only, which is currently fine, but...
# REMOVE IF APPLICATION IS MADE PUBLIC IN THE FUTURE

# Written for Python 3.12

import json
import re
from typing import Any, Final, Iterable

JsonObj = dict[str, dict[str, Any]]

def load_json(js: str) -> JsonObj:
    # Find first semicolon, here is where the HUE_ICONS_MAP lookup object should start
    first_semi: int = js.find('{')

    # Find last semicolon by following semicolon indentation
    # since there is a bunch of JavaScript after the map object.
    last_semi: int = first_semi + 1
    semi_indentation: int = 0

    while semi_indentation >= 0: # map object ends when semicolon indentation goes below 0
        last_semi += 1

        if js[last_semi] == '{':
            semi_indentation += 1
        if js[last_semi] == '}':
            semi_indentation -= 1

    json_obj_parts: list[str] = js[first_semi:(last_semi + 1)].splitlines()

    # json obj keys are not enclosed in parenthesis
    # we add them here before parsing as json
    for i in range(len(json_obj_parts)):
        json_obj_parts[i] = re.sub(
            r"([a-z]+):",
            lambda m: "\"" + m.group().removesuffix(':') + "\":",
            json_obj_parts[i],
            count=1 # so that only the leftmost instance is replaced
        )

    return json.loads(''.join(json_obj_parts))

class CSharpWriter:
    def __init__(self, indentation: str = "    ") -> None:
        self.indent: int = 0

        self._indentation: str = indentation
        self._content: list[str] = []

    def writeline(self, line: str = ""):
        self._content.append(self._indentation * self.indent)
        self._content.append(line)
        self._content.append('\n')

    def __str__(self) -> str:
        return "".join(self._content)

def snake_case2PascalCase(snake_case: str):
    parts: list[str] = snake_case.split('-')
    return "".join((p[0].upper() + p[1:]) for p in parts)

def write_cs(json_obj: JsonObj) -> str:
    writer: CSharpWriter = CSharpWriter()

    # write header
    writer.writeline("namespace NDiscoPlus.Code.NDPIcons;")
    writer.writeline()
    writer.writeline("public static class HueIcons")
    writer.writeline("{")

    # write icons
    writer.indent = 1
    for icon_name, icon_data in json_obj.items():
        cs_name: str = snake_case2PascalCase(icon_name)
        html_path: str = icon_data["path"]
        writer.writeline(f"public const string {cs_name} = \"<path d=\\\"{html_path}\\\"/>\";")
    writer.indent = 0

    # write end
    writer.writeline("}")

    return str(writer)

def main():
    print("Loading JavaScript from 'hass-hue-icons.js'...")
    javascript: str
    with open("hass-hue-icons.js", "r", encoding="utf-8") as f:
        javascript = f.read()

    print("Parsing JavaScript...")
    json_obj: JsonObj = load_json(javascript)
    
    print("Generating C#...")
    csharp: str = write_cs(json_obj)

    print("Saving C# into 'HueIcons.cs'...")
    with open("HueIcons.cs", "w", encoding="utf-8") as f:
        f.write(csharp)
        
    print("FINISHED!")

if __name__ == "__main__":
    main()
