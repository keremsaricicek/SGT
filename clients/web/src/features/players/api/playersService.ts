import type { SgtApi } from "@/shared/api/client";
import type {
  AddRestrictionInput,
  PlayerDetailsInput,
  PlayerRegistered,
  PlayerSummary,
  PlayerView,
} from "./types";
import { MIN_SEARCH_LENGTH } from "./types";

/** Oyuncu ekranları (Spec 005). */
export interface PlayersService {
  search(term: string): Promise<PlayerSummary[]>;
  get(playerId: string): Promise<PlayerView>;
  register(details: PlayerDetailsInput): Promise<PlayerRegistered>;
  issueCard(playerId: string, cardNumber: string, reasonCode: string): Promise<void>;
  cancelCard(playerId: string, reasonCode: string): Promise<void>;
  addRestriction(playerId: string, input: AddRestrictionInput): Promise<void>;
  removeRestriction(playerId: string, restrictionId: string, reasonCode: string): Promise<void>;
}

export function createPlayersService(api: SgtApi): PlayersService {
  const { client } = api;
  return {
    async search(term) {
      const q = term.trim();
      if (q.length < MIN_SEARCH_LENGTH) return [];
      return api.data<PlayerSummary[]>(client.GET("/api/v1/players", { params: { query: { q } } }));
    },
    get: (playerId) =>
      api.data<PlayerView>(client.GET("/api/v1/players/{playerId}", { params: { path: { playerId } } })),
    register: (details) => api.data<PlayerRegistered>(client.POST("/api/v1/players", { body: details })),
    issueCard: (playerId, cardNumber, reasonCode) =>
      api.done(
        client.POST("/api/v1/players/{playerId}/cards", {
          params: { path: { playerId } },
          body: { cardNumber, reasonCode },
        }),
      ),
    cancelCard: (playerId, reasonCode) =>
      api.done(
        client.POST("/api/v1/players/{playerId}/cards/cancel", { params: { path: { playerId } }, body: { reasonCode } }),
      ),
    addRestriction: (playerId, input) =>
      api.done(client.POST("/api/v1/players/{playerId}/restrictions", { params: { path: { playerId } }, body: input })),
    removeRestriction: (playerId, restrictionId, reasonCode) =>
      api.done(
        client.POST("/api/v1/players/{playerId}/restrictions/{restrictionId}/remove", {
          params: { path: { playerId, restrictionId } },
          body: { reasonCode },
        }),
      ),
  };
}
