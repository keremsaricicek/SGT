export interface ContrastPairs {
  readonly themes: readonly string[];
  readonly text: readonly (readonly [string, string])[];
  readonly nonText: readonly (readonly [string, string])[];
}

export interface ContrastFailure {
  readonly theme: string;
  readonly fg: string;
  readonly bg: string;
  readonly ratio: number;
  readonly min: number;
  readonly missing?: boolean;
}

export function luminance(hex: string): number;
export function contrastRatio(a: string, b: string): number;
export function contrastFailures(tokens: unknown, pairs: ContrastPairs): ContrastFailure[];
