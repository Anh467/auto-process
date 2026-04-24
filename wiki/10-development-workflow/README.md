# 10 - Development Workflow & Code Formatting

## Tổng quan

Dự án áp dụng **auto-format on commit** để đảm bảo code quality và consistency. Thay vì chặn commit, hệ thống sẽ tự động format code trước khi commit.

### Công cụ sử dụng

| Khu vực               | Công cụ         | Mô tả                      |
| --------------------- | --------------- | -------------------------- |
| **Client (Frontend)** | Prettier        | Format JS/TS/JSX/TSX files |
| **Server (Backend)**  | dotnet format   | Format C# files            |
| **Git Hook**          | Pre-commit hook | Tự động chạy khi commit    |

### Đặc điểm

- ✅ **Không chặn commit** - Hook luôn trả về exit code 0
- ✅ **Tự động format** - Code được format trước khi commit
- ✅ **Client-side** - Chạy trên máy developer, không cần CI
- ✅ **Chỉ format file đã staged** - Không ảnh hưởng file khác

---

## Quick Start (Cho Developer Mới)

### 1. Cài đặt requirements

```bash
# 1. Node.js & npm (cho Prettier)
# Tải từ: https://nodejs.org/

# 2. .NET SDK 6.0+ (cho dotnet format)
# Tải từ: https://dotnet.microsoft.com/download
```

### 2. Chạy setup script

```bash
# Windows
.\scripts\setup-hooks.bat

# Mac/Linux
chmod +x scripts/setup-hooks.sh
./scripts/setup-hooks.sh
```

### 3. Verify setup

```bash
# Kiểm tra hook đã được cài đặt
ls .git/hooks/pre-commit  # Mac/Linux
dir .git\hooks\pre-commit # Windows

# Hook sẽ tự động chạy khi bạn commit
git add .
git commit -m "test commit"
```

---

## Chi tiết Kỹ thuật

### Kiến trúc Pre-commit Hook

```
┌─────────────────────────────────────────────────────────────┐
│                     git commit -m "msg"                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Pre-commit Hook                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 1. Lấy danh sách file đã staged                     │    │
│  │ 2. Phân loại file theo extension                    │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
┌─────────────────────────┐     ┌─────────────────────────┐
│   Client Files          │     │   Server Files          │
│   (*.js,*.ts,*.tsx)     │     │   (*.cs)                │
│                         │     │                         │
│   → npx prettier        │     │   → dotnet format       │
│      --write            │     │      --include          │
└─────────────────────────┘     └─────────────────────────┘
              │                               │
              └───────────────┬───────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              git add (re-stage formatted files)              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Commit hoàn tất                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Setup chi tiết cho Backend (.NET)

### 1. Kiểm tra dotnet format

```bash
# dotnet format đã được include trong .NET 6+ SDK
# Kiểm tra version
dotnet format --version

# Nếu chưa có, cài đặt global tool
dotnet tool install -g dotnet-format
```

### 2. Cấu hình EditorConfig

Tạo file `.editorconfig` ở root của solution:

```editorconfig
# EditorConfig giúp đồng bộ coding style across IDEs
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# C# files
[*.cs]
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

# Use language keywords instead of framework types
dotnet_style_predefined_type_for_locals_parameters_members = true:suggestion
dotnet_style_predefined_type_for_member_access = true:suggestion

# Naming conventions
dotnet_naming_rule.interface_should_be_prefixed_with_i.severity = suggestion
dotnet_naming_rule.interface_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_prefixed_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.interface.applicable_accessibilities = public, internal

dotnet_naming_style.begins_with_i.capitalization = pascal_case
dotnet_naming_style.begins_with_i.required_prefix = I

# Private fields should be prefixed with _
dotnet_naming_rule.private_fields_underscore.severity = suggestion
dotnet_naming_rule.private_fields_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_underscore.style = underscore_prefix

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.underscore_prefix.capitalization = camel_case
dotnet_naming_style.underscore_prefix.required_prefix = _
```

### 3. Cấu hình EditorConfig

File `.editorconfig` đã được cấu hình sẵn ở root project.

---

## Setup chi tiết cho Frontend (Client)

### 1. Cài đặt dependencies (nếu cần chạy manual)

```bash
cd client

npm install --save-dev prettier
# hoặc
yarn add --dev prettier
```

### 2. Cấu hình Prettier

File `.prettierrc` đã được cấu hình sẵn trong `client/`:

```json
{
  "semi": true,
  "trailingComma": "all",
  "singleQuote": true,
  "printWidth": 80,
  "tabWidth": 2,
  "useTabs": false,
  "bracketSpacing": true,
  "arrowParens": "always",
  "endOfLine": "lf"
}
```

### 3. File ignore

File `.prettierignore` đã được cấu hình sẵn trong `client/`.

---

## Setup cho Full-Stack Project (Auto-format)

Project đã có sẵn pre-commit hook ở `.git/hooks/pre-commit` để auto-format cả client và server.

### 1. Cài đặt hook (một lần duy nhất)

```bash
# Windows
.\scripts\setup-hooks.bat

# Mac/Linux
chmod +x scripts/setup-hooks.sh
./scripts/setup-hooks.sh
```

### 2. Script format manual (nếu cần)

Tạo file `scripts/format-staged.sh`:

```bash
#!/bin/bash

# Format Backend (.NET)
echo "=== Formatting Backend (.NET) ==="
STAGED_CS_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep '\.cs$' | tr '\n' ' ')

if [ -n "$STAGED_CS_FILES" ]; then
    echo "Formatting .cs files..."
    dotnet format whitespace --include $STAGED_CS_FILES 2>/dev/null || true
    dotnet format style --include $STAGED_CS_FILES --severity warn 2>/dev/null || true
    git add $STAGED_CS_FILES
fi

# Format Frontend (Prettier)
echo "=== Formatting Frontend (Prettier) ==="
STAGED_TS_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.(ts|tsx|js|jsx)$' | tr '\n' ' ')

if [ -n "$STAGED_TS_FILES" ]; then
    echo "Formatting TypeScript/JavaScript files..."
    cd client
    npx prettier --write $STAGED_TS_FILES
    git add $STAGED_TS_FILES
    cd ..
fi

echo "=== Format complete ==="
```

### 3. Cấu hình root package.json (optional)

```json
{
  "scripts": {
    "format": "npm run format:backend && npm run format:frontend",
    "format:backend": "dotnet format",
    "format:frontend": "cd frontend && npm run format",
    "prepare": "husky install"
  },
  "devDependencies": {
    "husky": "^8.0.0",
    "lint-staged": "^13.0.0"
  },
  "lint-staged": {
    "*.cs": ["dotnet format --include"],
    "*.{ts,tsx,js,jsx}": ["prettier --write"]
  }
}
```

---

## CI/CD Integration

### GitHub Actions

Tạo file `.github/workflows/code-quality.yml`:

```yaml
name: Code Quality

on:
  pull_request:
    branches: [main, develop]

jobs:
  format-check:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: "10.0.x"

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: "18.x"

      - name: Check .NET format
        run: |
          dotnet format --verify-no-changes --verbosity diagnostic

      - name: Install frontend dependencies
        run: |
          cd frontend
          npm ci

      - name: Check Prettier format
        run: |
          cd frontend
          npx prettier --check "src/**/*.{ts,tsx,js,jsx,json,css,scss,md}"
```

---

## Developer Guide

### Khi làm việc hàng ngày

1. **Code như bình thường** - Không cần chạy format thủ công
2. **Khi commit** - Hook sẽ tự động format các file đã staged
3. **Nếu có conflict** - Resolve conflict rồi commit lại
4. **Review changes** - Kiểm tra changes sau khi format trước khi push

### Format toàn bộ project (khi cần)

```bash
# Backend (.NET)
dotnet format server

# Frontend (Client)
cd client
npx prettier --write "src/**/*.{js,jsx,ts,tsx,json,css,scss,md}"

# Hoặc format toàn bộ
dotnet format
cd client && npx prettier --write .
```

### Bypass hook (trường hợp đặc biệt)

```bash
# Bypass pre-commit hook
git commit --no-verify -m "Your message"

# Lưu ý: Chỉ sử dụng khi thực sự cần thiết
# Ví dụ: hotfix, emergency commit
```

### Kiểm tra hook đang active

```bash
# Xem nội dung hook
cat .git/hooks/pre-commit

# Test hook manually
.git/hooks/pre-commit
```

---

## Troubleshooting

### Lỗi: dotnet format không tìm thấy

```bash
# Kiểm tra .NET SDK
dotnet --version

# Cài đặt dotnet format (nếu cần)
dotnet tool install -g dotnet-format

# Hoặc sử dụng local tool
dotnet new tool-manifest
dotnet tool install dotnet-format
```

### Lỗi: Prettier không format đúng

```bash
# Kiểm tra Node.js
node --version
npm --version

# Cài đặt Prettier trong client
cd client
npm install --save-dev prettier

# Clear Prettier cache
npx prettier --clear-cache
```

### Lỗi: Pre-commit hook không chạy

```bash
# Kiểm tra hook có tồn tại
ls -la .git/hooks/pre-commit  # Mac/Linux
dir .git\hooks\pre-commit     # Windows

# Chạy lại setup script
# Windows
.\scripts\setup-hooks.bat

# Mac/Linux
./scripts/setup-hooks.sh

# Cấp quyền (Mac/Linux)
chmod +x .git/hooks/pre-commit
```

### Lỗi: Hook chạy nhưng không format

1. **Kiểm tra file có trong staged area không:**

   ```bash
   git diff --cached --name-only
   ```

2. **Kiểm tra log khi commit:**

   ```bash
   # Commit với verbose output
   git commit -m "test" 2>&1 | tee commit.log
   ```

3. **Test hook manually:**
   ```bash
   # Chạy hook thủ công
   .git/hooks/pre-commit
   ```

---

## Best Practices

1. **Đừng commit file đã format sẵn** - Hãy để hook làm việc
2. **Review changes sau format** - Đôi khi formatter có thể thay đổi không mong muốn
3. **Giữ hook nhẹ** - Chỉ format, không lint nặng
4. **Document cho team** - Đảm bảo mọi dev đều hiểu workflow
5. **Cài đặt requirements** - Đảm bảo Node.js và .NET SDK đã cài
6. **Commit nhỏ, thường xuyên** - Giúp hook chạy nhanh hơn

---

## Tài liệu tham khảo

- [dotnet format documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
- [Prettier documentation](https://prettier.io/docs/en/)
- [Git Hooks documentation](https://git-scm.com/book/en/v2/Customizing-Git-Git-Hooks)

---

## Lịch sử thay đổi

| Ngày       | Thay đổi                                                                     | Người thực hiện |
| ---------- | ---------------------------------------------------------------------------- | --------------- |
| 2026-04-25 | Áp dụng auto-format on commit cho client (Prettier) + server (dotnet format) | Team            |
