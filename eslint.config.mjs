import js from "@eslint/js";
import globals from "globals";

const sharedIgnores = [
  "**/bin/**",
  "**/obj/**",
  "output/**",
  "src/PrompterOne.Shared/wwwroot/vendor/**"
];

export default [
  {
    ignores: sharedIgnores
  },
  {
    ...js.configs.recommended,
    files: [
      "src/PrompterOne.Shared/wwwroot/**/*.js",
      "tests/PrompterOne.Web.UITests/**/*.js"
    ],
    languageOptions: {
      ecmaVersion: "latest",
      globals: {
        ...globals.browser,
        ...globals.node
      },
      sourceType: "module"
    },
    rules: {
      "no-unused-vars": [
        "error",
        {
          argsIgnorePattern: "^_",
          caughtErrorsIgnorePattern: "^_",
          ignoreRestSiblings: true
        }
      ]
    }
  },
  {
    files: [
      "src/PrompterOne.Shared/wwwroot/localization/**/*.js",
      "src/PrompterOne.Shared/wwwroot/media/**/*.js",
      "src/PrompterOne.Shared/wwwroot/teleprompter/**/*.js",
      "src/PrompterOne.Shared/wwwroot/theme/**/*.js",
      "tests/PrompterOne.Web.UITests/**/*.js"
    ],
    languageOptions: {
      ecmaVersion: "latest",
      globals: {
        ...globals.browser,
        ...globals.node
      },
      sourceType: "script"
    }
  }
];
