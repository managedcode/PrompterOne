export default {
  extends: ["stylelint-config-standard"],
  ignoreFiles: [
    "**/bin/**",
    "**/obj/**",
    "output/**",
    "src/PrompterOne.Shared/wwwroot/bootstrap/**",
    "src/PrompterOne.Shared/wwwroot/vendor/**"
  ],
  rules: {
    "alpha-value-notation": null,
    "at-rule-empty-line-before": null,
    "color-function-notation": null,
    "color-function-alias-notation": null,
    "declaration-block-no-redundant-longhand-properties": null,
    "declaration-block-single-line-max-declarations": null,
    "declaration-property-value-keyword-no-deprecated": null,
    "declaration-property-value-no-unknown": null,
    "media-feature-range-notation": null,
    "no-descending-specificity": null,
    "number-max-precision": null,
    "property-no-deprecated": null,
    "property-no-vendor-prefix": null,
    "selector-attribute-quotes": null,
    "selector-class-pattern": null,
    "selector-not-notation": null,
    "selector-pseudo-element-no-unknown": null,
    "value-keyword-case": null
  }
};
