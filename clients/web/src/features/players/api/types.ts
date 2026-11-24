import type { Schemas } from "@/shared/api/client";

/**
 * Oyuncu yanıt tipleri (Sgt.Modules.CasinoPlayer.Application); sözleşmeden üretilir.
 * Belge numarası sunucuda maskelenir (Spec 005 KK4); istemci tam numarayı hiç almaz.
 */
export type PlayerSummary = Schemas["PlayerSummary"];
export type RestrictionView = Schemas["RestrictionView"];
export type PlayerView = Schemas["PlayerView"];
export type PlayerRegistered = Schemas["PlayerRegistered"];

export type PlayerDetailsInput = Schemas["PlayerDetailsRequest"];
export type AddRestrictionInput = Schemas["AddRestrictionRequest"];
export type DocumentType = Schemas["DocumentType"];
export type RestrictionType = Schemas["RestrictionType"];

export const DOCUMENT_TYPES: readonly DocumentType[] = ["NationalId", "Passport", "DriverLicense"];
export const RESTRICTION_TYPES: readonly RestrictionType[] = ["Banned", "SelfExcluded"];

/** Arama terimi sunucuda en az 2 karakter olmalıdır (SearchPlayersHandler). */
export const MIN_SEARCH_LENGTH = 2;
