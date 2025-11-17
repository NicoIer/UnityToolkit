# Copyright (c) 2023 NicoIer and Contributors.
# Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
# Copyright (c) 2023 NicoIer and Contributors.
# Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
# Copyright (c) 2023 NicoIer and Contributors.
# Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
import os
import re

def add_copyright_to_file(file_path, copyright_info, comment_prefix=None):
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()
    if copyright_info in content:
        return
    if not comment_prefix:
        new_content = copyright_info + "\n" + content
    elif isinstance(comment_prefix, tuple) and len(comment_prefix) == 2:
        # HTML/XML comment block
        new_content = (
                f"{comment_prefix[0]}\n{copyright_info}\n{comment_prefix[1]}\n" + content
        )
    else:
        # Line comments
        copyright_lines = copyright_info.splitlines()
        comment_block = "\n".join(f"{comment_prefix}{line}" for line in copyright_lines)
        new_content = comment_block + "\n" + content
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(new_content)

def is_source_file(filename):
    return filename.endswith((".py", ".cs", ".js", ".java", ".html", ".xml"))

def get_copyright_from_editorconfig(editorconfig_path):
    with open(editorconfig_path, "r", encoding="utf-8") as f:
        content = f.read()
        match = re.search(r'file_header_template\s*=\s*(.+)', content)
        if match:
            return match.group(1).replace("\\n", "\n").strip()
    return ""

def get_comment_prefix(filename):
    if filename.endswith(".py"):
        return "# "
    elif filename.endswith(".cs") or filename.endswith(".js") or filename.endswith(".java"):
        return "// "
    elif filename.endswith(".html") or filename.endswith(".xml"):
        return "<!-- ", " -->"
    else:
        return None

def find_editorconfig(start_path="."):
    # 从start_path以及其上层递归查找.editorconfig文件
    current = os.path.abspath(start_path)
    while True:
        cfg_path = os.path.join(current, ".editorconfig")
        if os.path.isfile(cfg_path):
            return cfg_path
        parent = os.path.dirname(current)
        if parent == current:
            break
        current = parent
    return None

def process_files(root_path, copyright_info):
    for dirpath, dirnames, filenames in os.walk(root_path):
        for filename in filenames:
            if is_source_file(filename):
                file_path = os.path.join(dirpath, filename)
                comment_prefix = get_comment_prefix(filename)
                print(f"Processing file: {file_path}")
                add_copyright_to_file(file_path, copyright_info, comment_prefix)

if __name__ == "__main__":
    # 从上一级目录开始查找.editorconfig文件
    editorconfig_path = find_editorconfig(os.path.abspath(os.path.join(os.getcwd(), '..')))
    if not editorconfig_path:
        print("No .editorconfig found!")
        exit(1)
    copyright_info = get_copyright_from_editorconfig(editorconfig_path)
    if not copyright_info:
        print("No copyright info found in .editorconfig!")
        exit(1)
    # 给当前目录及其子目录递归地添加版权信息
    process_files("../", copyright_info)