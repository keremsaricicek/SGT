import { execFileSync } from "node:child_process";
import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Icon } from "./Icon";

describe("Fluent simgeleri (Spec 013)", () => {
  it("üretilmiş simge modülü @fluentui/svg-icons ile günceldir", () => {
    expect(() => execFileSync(process.execPath, ["scripts/build-icons.mjs", "--check"], { stdio: "pipe" })).not.toThrow();
  });

  it("simge ekran okuyucudan gizlenir ve metin rengini kullanır", () => {
    const { container } = render(<Icon name="search" />);
    const svg = container.querySelector("svg");
    expect(svg?.getAttribute("aria-hidden")).toBe("true");
    expect(svg?.getAttribute("fill")).toBe("currentColor");
    expect(svg?.querySelectorAll("path").length).toBeGreaterThan(0);
  });
});
