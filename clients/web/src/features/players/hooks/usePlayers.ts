"use client";

import { useApiMutation } from "@/shared/hooks/useApiMutation";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { createPlayersService } from "../api/playersService";
import type { AddRestrictionInput, PlayerDetailsInput, PlayerRegistered } from "../api/types";
import { MIN_SEARCH_LENGTH } from "../api/types";

/** Arama; terim kısa ise istek atılmaz. */
export function usePlayerSearch(term: string) {
  const q = term.trim();
  return useApiQuery(q.length >= MIN_SEARCH_LENGTH ? `players?q=${q}` : null, (api) => createPlayersService(api).search(q));
}

export function usePlayer(playerId: string) {
  return useApiQuery(`players/${playerId}`, (api) => createPlayersService(api).get(playerId));
}

export function useRegisterPlayer(onDone?: (registered: PlayerRegistered) => void) {
  return useApiMutation((api, details: PlayerDetailsInput) => createPlayersService(api).register(details), onDone);
}

export type PlayerChange =
  | { readonly kind: "issueCard"; readonly cardNumber: string; readonly reasonCode: string }
  | { readonly kind: "cancelCard"; readonly reasonCode: string }
  | { readonly kind: "addRestriction"; readonly input: AddRestrictionInput }
  | { readonly kind: "removeRestriction"; readonly restrictionId: string; readonly reasonCode: string };

export function usePlayerChange(playerId: string, onDone?: () => void) {
  return useApiMutation((api, change: PlayerChange) => {
    const players = createPlayersService(api);
    switch (change.kind) {
      case "issueCard":
        return players.issueCard(playerId, change.cardNumber, change.reasonCode);
      case "cancelCard":
        return players.cancelCard(playerId, change.reasonCode);
      case "addRestriction":
        return players.addRestriction(playerId, change.input);
      case "removeRestriction":
        return players.removeRestriction(playerId, change.restrictionId, change.reasonCode);
    }
  }, onDone);
}
