# TaskFlow Frontend Design System

**Status:** Initial shared baseline  
**Framework:** Bootstrap 5.3 + custom CSS  
**Applies to:** All pages in `TaskFlow.Web`

## 1. Design principles

1. Keep interfaces clear, calm, and practical.
2. Prefer familiar patterns over decorative novelty.
3. Use spacing and alignment to establish hierarchy.
4. Avoid excessive gradients, glass effects, large shadows, and unnecessary animation.
5. Use consistent components rather than page-specific copies.
6. Support keyboard navigation, visible focus, and responsive layouts.

## 2. Brand and theme tokens

The source of truth is `wwwroot/css/design-tokens.css`.

| Token | Light | Dark |
|---|---|---|
| Page background | `#F8F9FA` | `#121212` |
| Surface | `#FFFFFF` | `#1C1C1F` |
| Main text | `#171923` | `#F2F3F5` |
| Muted text | `#687080` | `#A3A7B0` |
| Border | `#E4E7EC` | `#34343B` |
| Primary accent | `#505AC9` | `#7C84E8` |
| Font | Inter | Inter |

The selected primary brand color is `#505AC9`. The dark theme uses a lighter accessible variation of the same hue so controls remain distinguishable against dark surfaces.

Do not hardcode colors in feature styles. Use `var(--tf-primary)`, `var(--tf-surface)`, `var(--tf-text)`, `var(--tf-border)`, and the other shared tokens.

## 3. Typography

- Font family: Inter.
- Page heading: approximately 1.35–1.7rem, semibold.
- Body: 14px base in the application shell.
- Supporting text: use `--tf-text-muted`.
- Use sentence case for labels and buttons.
- Avoid using multiple font families.

## 4. Shape, spacing, and elevation

- Small radius: 6px.
- Standard controls: 10px.
- Cards/panels: 14px.
- Larger containers: 16px.
- Use the spacing tokens in `design-tokens.css`.
- Use subtle borders and low-elevation shadows. Avoid heavy shadows.
- Do not make every element pill-shaped. Reserve full pills for status badges and compact tags.

## 5. Shared shell

Use `_Layout.cshtml` for every normal application page. It composes:

- `_Sidebar.cshtml`
- `_Navbar.cshtml`
- `@RenderBody()`
- `_Footer.cshtml`

Do not duplicate the shell in feature views. The sidebar links are route-aware and use MVC Tag Helpers. Update their controller/action names if the actual project uses different routes.

## 6. Component conventions

Use the shared classes below before writing new styles:

| Component | Class |
|---|---|
| Card | `.tf-card` |
| Card header | `.tf-card-header` |
| Card body | `.tf-card-body` |
| Page title | `.tf-page-heading` |
| Page subtitle | `.tf-page-subtitle` |
| Primary button | `.tf-btn-primary` |
| Outline button | `.tf-btn-outline` |
| Form control | `.tf-form-control` |
| Primary badge | `.tf-badge tf-badge-primary` |
| Success badge | `.tf-badge tf-badge-success` |
| Warning badge | `.tf-badge tf-badge-warning` |
| Danger badge | `.tf-badge tf-badge-danger` |
| Muted text | `.tf-muted` |
| Data table | `.tf-table` |
| Empty state | `.tf-empty-state` |

Bootstrap grid, spacing, flex, and utility classes may be used. Shared visual decisions (colors, radius, shadows, typography) must still come from TaskFlow tokens.

## 7. Layout and responsive behavior

- Desktop sidebar width: 256px.
- Collapsed desktop sidebar: 76px.
- Mobile breakpoint: 992px.
- Mobile navigation opens as a drawer with a backdrop.
- Keep page content fluid and avoid fixed-width content panels.
- Tables and wide data visualizations should use a responsive wrapper such as `.table-responsive`.

## 8. Dark mode

Use the theme toggle in the shared navbar. Theme preference is stored in `localStorage` under `taskflow-theme`.

Feature styles must use semantic tokens rather than separate hardcoded light/dark colors. Example:

```css
.project-panel {
    background: var(--tf-surface);
    border: 1px solid var(--tf-border);
    color: var(--tf-text);
}
```

## 9. Razor and JavaScript rules

- Use MVC Tag Helpers for internal routes (`asp-controller`, `asp-action`, `asp-route-*`).
- Do not use `href="#"` as a substitute for an unfinished action. Use a disabled button or omit the action until implemented.
- Load feature scripts through `@section Scripts`.
- Load feature styles through `@section Styles`.
- Do not load Bootstrap's CSS or JavaScript more than once.
- Keep global layout behavior in `layout.js`; keep theme behavior in `theme.js`.
- Do not put feature-specific behavior in the global layout script.

## 10. Accessibility checklist

- Use semantic headings in order.
- Give icon-only buttons accessible labels.
- Preserve visible keyboard focus.
- Ensure status is not communicated by color alone.
- Use labels for form fields.
- Use sufficient contrast for text and controls.
- Keep mobile navigation keyboard-operable.
- Add `aria-current="page"` to the active navigation item.

## 11. Team contribution rules

Before adding a new component:

1. Search `components.css` for an existing equivalent.
2. Reuse the existing component and tokens if possible.
3. If it is genuinely new and reusable, add a `.tf-` class to `components.css`.
4. Document its intended use here.
5. Avoid global element selectors for feature-specific styling.
6. Do not edit shared tokens casually; coordinate changes with the team.

## 12. Reusable AI prompt for every developer

> You are implementing a feature inside the existing TaskFlow ASP.NET Core MVC application. Follow `DESIGN-SYSTEM.md` and the shared files in `wwwroot/css` and `wwwroot/js` as the source of truth. Use Bootstrap 5.3 with the existing TaskFlow custom classes. Do not introduce Tailwind or another CSS framework. Reuse `_Layout.cshtml`, existing partials, design tokens, and shared components. Do not recreate the sidebar, navbar, footer, or theme switch. Use MVC Tag Helpers for internal navigation. Support light and dark themes through CSS variables, responsive layouts, keyboard navigation, and visible focus states. Keep the UI simple and professional; avoid unnecessary gradients, glass effects, excessive rounding, and decorative animation. If a reusable component is missing, create it using existing tokens and add it to `components.css`; do not invent a new visual language. Do not change shared design tokens or layout unless explicitly requested. Return complete, project-compatible Razor/CSS/JavaScript changes and identify any controller, model, route, or package assumptions.
