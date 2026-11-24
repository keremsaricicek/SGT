import type { SgtApi } from "@/shared/api/client";

/** GET /api/v1/identity/me/menu öğesi (Identity MenuItem): görülebilen ekran ve yapılabilecek eylemler. */
export interface MenuItem {
  readonly screen: string;
  readonly actions: readonly string[];
}

export async function fetchMyMenu(api: SgtApi): Promise<MenuItem[]> {
  return api.data<MenuItem[]>(api.client.GET("/api/v1/identity/me/menu"));
}

/**
 * Menüye göre görünürlük kararı. Sunucu her istekte yetkiyi yeniden denetler (Spec 001 KK10);
 * buradaki karar yalnızca arayüzde gizleme içindir.
 */
export class AccessPolicy {
  private readonly byScreen: ReadonlyMap<string, ReadonlySet<string>>;

  constructor(menu: readonly MenuItem[]) {
    this.byScreen = new Map(menu.map((item) => [item.screen, new Set(item.actions)]));
  }

  canView(screen: string): boolean {
    return this.byScreen.has(screen);
  }

  can(screen: string, action: string): boolean {
    return this.byScreen.get(screen)?.has(action) ?? false;
  }

  get screens(): string[] {
    return [...this.byScreen.keys()];
  }
}
