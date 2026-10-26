import { describe, expect, it } from "vitest";
import { fakeFetch, testApi } from "@/test/fakeApi";
import { createPlayersService } from "./playersService";
import type { PlayerView } from "./types";

const PLAYER_ID = "55555555-5555-4555-8555-555555555555";
const RESTRICTION_ID = "66666666-6666-4666-8666-666666666666";

/** Sentetik oyuncu: gerçek kişi verisi değildir; belge numarası sunucudan maskeli gelir. */
const PLAYER: PlayerView = {
  playerId: PLAYER_ID,
  playerNumber: "P000001",
  firstName: "Test",
  lastName: "Oyuncu",
  birthDate: "1990-01-01",
  nationality: "XX",
  documentType: "Passport",
  maskedDocumentNumber: "*****1234",
  phone: null,
  activeCardNumber: null,
  isRestricted: false,
  activeRestrictions: [],
};

describe("PlayersService", () => {
  it("2 karakterden kısa aramada istek atmaz", async () => {
    const fake = fakeFetch([]);
    await expect(createPlayersService(testApi(fake.fetch)).search(" a ")).resolves.toEqual([]);
    expect(fake.requests).toHaveLength(0);
  });

  it("arama terimini q parametresiyle gönderir", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/players", body: [] }]);
    await createPlayersService(testApi(fake.fetch)).search(" P000001 ");
    expect(fake.requests[0]?.url.searchParams.get("q")).toBe("P000001");
  });

  it("oyuncu kartını maskeli belge numarasıyla döner", async () => {
    const fake = fakeFetch([{ method: "GET", path: `/api/v1/players/${PLAYER_ID}`, body: PLAYER }]);
    const player = await createPlayersService(testApi(fake.fetch)).get(PLAYER_ID);
    expect(player.maskedDocumentNumber).toBe("*****1234");
  });

  it("oyuncu kaydeder ve oyuncu numarasını döner", async () => {
    const fake = fakeFetch([
      { method: "POST", path: "/api/v1/players", status: 201, body: { playerId: PLAYER_ID, playerNumber: "P000001" } },
    ]);
    const details = {
      firstName: "Test",
      lastName: "Oyuncu",
      birthDate: "1990-01-01",
      nationality: "XX",
      documentType: "Passport" as const,
      documentNumber: "TEST0001",
      phone: null,
      reasonCode: "NEW_PLAYER",
    };
    const registered = await createPlayersService(testApi(fake.fetch)).register(details);
    expect(registered.playerNumber).toBe("P000001");
    expect(fake.requests[0]?.body).toEqual(details);
  });

  it("kart basar ve iptal eder", async () => {
    const fake = fakeFetch([
      { method: "POST", path: `/api/v1/players/${PLAYER_ID}/cards` },
      { method: "POST", path: `/api/v1/players/${PLAYER_ID}/cards/cancel` },
    ]);
    const players = createPlayersService(testApi(fake.fetch));
    await players.issueCard(PLAYER_ID, "C-TEST-01", "NEW_CARD");
    await players.cancelCard(PLAYER_ID, "LOST_CARD");
    expect(fake.requests[0]?.body).toEqual({ cardNumber: "C-TEST-01", reasonCode: "NEW_CARD" });
    expect(fake.requests[1]?.body).toEqual({ reasonCode: "LOST_CARD" });
  });

  it("kısıtlama ekler ve kaldırır", async () => {
    const fake = fakeFetch([
      { method: "POST", path: `/api/v1/players/${PLAYER_ID}/restrictions`, status: 201, body: { id: RESTRICTION_ID } },
      { method: "POST", path: `/api/v1/players/${PLAYER_ID}/restrictions/${RESTRICTION_ID}/remove` },
    ]);
    const players = createPlayersService(testApi(fake.fetch));
    await players.addRestriction(PLAYER_ID, { type: "SelfExcluded", note: null, until: null, reasonCode: "PLAYER_REQUEST" });
    await players.removeRestriction(PLAYER_ID, RESTRICTION_ID, "MANAGER_REQUEST");
    expect(fake.requests[0]?.body).toEqual({ type: "SelfExcluded", note: null, until: null, reasonCode: "PLAYER_REQUEST" });
    expect(fake.requests[1]?.body).toEqual({ reasonCode: "MANAGER_REQUEST" });
  });
});
