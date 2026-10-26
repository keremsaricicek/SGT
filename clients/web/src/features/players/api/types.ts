import type { Schemas } from "@/shared/api/client";

/**
 * Oyuncu yanıt tipleri (Sgt.Modules.CasinoPlayer.Application). Sözleşmede yanıt şeması yok; elle yazıldı.
 * Belge numarası sunucuda maskelenir (Spec 005 KK4); istemci tam numarayı hiç almaz.
 */
export interface PlayerSummary {
  readonly playerId: string;
  readonly playerNumber: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly activeCardNumber: string | null;
  readonly isRestricted: boolean;
}

export interface RestrictionView {
  readonly id: string;
  readonly type: string;
  readonly note: string | null;
  readonly until: string | null;
  readonly addedBy: string;
  readonly addedAt: string;
}

export interface PlayerView {
  readonly playerId: string;
  readonly playerNumber: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly birthDate: string;
  readonly nationality: string;
  readonly documentType: string;
  readonly maskedDocumentNumber: string;
  readonly phone: string | null;
  readonly activeCardNumber: string | null;
  readonly isRestricted: boolean;
  readonly activeRestrictions: readonly RestrictionView[];
}

export interface PlayerRegistered {
  readonly playerId: string;
  readonly playerNumber: string;
}

export type PlayerDetailsInput = Schemas["PlayerDetailsRequest"];
export type AddRestrictionInput = Schemas["AddRestrictionRequest"];
export type DocumentType = Schemas["DocumentType"];
export type RestrictionType = Schemas["RestrictionType"];

export const DOCUMENT_TYPES: readonly DocumentType[] = ["NationalId", "Passport", "DriverLicense"];
export const RESTRICTION_TYPES: readonly RestrictionType[] = ["Banned", "SelfExcluded"];

/** Arama terimi sunucuda en az 2 karakter olmalıdır (SearchPlayersHandler). */
export const MIN_SEARCH_LENGTH = 2;
