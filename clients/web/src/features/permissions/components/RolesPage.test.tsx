import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { fakeFetch, problem, testWrapper } from "@/test/fakeApi";
import { RolesPage } from "./RolesPage";

const ROLES = [{ id: "44444444-4444-4444-8444-444444444444", name: "Slot müdürü", permissions: ["slot.floor:view"] }];

describe("RolesPage", () => {
  it("menüde ekran yoksa içerik yerine yetki mesajı gösterir", () => {
    const fake = fakeFetch([]);
    render(<RolesPage />, { wrapper: testWrapper(fake.fetch, []) });
    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeTruthy();
    expect(fake.requests).toHaveLength(0);
  });

  it("operate eylemi yoksa rol oluşturma formunu gizler", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/identity/roles", body: ROLES }]);
    render(<RolesPage />, { wrapper: testWrapper(fake.fetch, [{ screen: "admin.roles", actions: ["view"] }]) });
    await screen.findByText("Slot müdürü");
    expect(screen.queryByRole("button", { name: "Rol oluştur" })).toBeNull();
  });

  it("gerekçe kodu olmadan göndermez; API hatasını başlık ve ayrıntıyla gösterir", async () => {
    const fake = fakeFetch([
      { method: "GET", path: "/api/v1/identity/roles", body: ROLES },
      { method: "POST", path: "/api/v1/identity/roles", status: 403, body: problem("Identity.NotSuperAdmin", "Bu işlemi yalnızca süper admin yapabilir.", 403) },
    ]);
    render(<RolesPage />, { wrapper: testWrapper(fake.fetch, [{ screen: "admin.roles", actions: ["operate", "view"] }]) });
    await screen.findByText("Slot müdürü");

    fireEvent.change(screen.getByLabelText("Rol adı"), { target: { value: "Vardiya şefi" } });
    fireEvent.click(screen.getByRole("button", { name: "Rol oluştur" }));
    expect(screen.getAllByText("Gerekçe kodu zorunludur.").length).toBeGreaterThan(0);
    expect(fake.requests.filter((r) => r.method === "POST")).toHaveLength(0);

    const [reasonInput] = screen.getAllByPlaceholderText("Zorunlu; ör. ROLE_SETUP, CORRECTION");
    fireEvent.change(reasonInput!, { target: { value: "role_setup" } });
    fireEvent.click(screen.getByRole("button", { name: "Rol oluştur" }));

    await screen.findByText("Bu işlemi yalnızca süper admin yapabilir.");
    expect(screen.getByText("Identity.NotSuperAdmin")).toBeTruthy();
    await waitFor(() =>
      expect(fake.requests.find((r) => r.method === "POST")?.body).toEqual({ name: "Vardiya şefi", reasonCode: "ROLE_SETUP" }),
    );
  });
});
