"use client";

import { useApiMutation } from "@/shared/hooks/useApiMutation";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { createDeadLettersService } from "../api/deadLettersService";

export function useDeadLetters() {
  return useApiQuery("integration/dead-letters", (api) => createDeadLettersService(api).list());
}

export function useReprocess(onDone?: () => void) {
  return useApiMutation(
    (api, input: { module: string; eventId: string }) => createDeadLettersService(api).reprocess(input.module, input.eventId),
    onDone,
  );
}
