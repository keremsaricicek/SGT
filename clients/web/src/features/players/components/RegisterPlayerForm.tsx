"use client";

import { useState } from "react";
import type { ChangeEvent } from "react";
import { useT } from "@/shared/i18n/I18nProvider";
import { ActionForm } from "@/shared/ui/ActionForm";
import { SelectField, TextField } from "@/shared/ui/form";
import type { DocumentType, PlayerRegistered } from "../api/types";
import { DOCUMENT_TYPES } from "../api/types";
import { useRegisterPlayer } from "../hooks/usePlayers";

const EMPTY = {
  firstName: "",
  lastName: "",
  birthDate: "",
  nationality: "",
  documentType: "NationalId" as DocumentType,
  documentNumber: "",
  phone: "",
};

type FormState = typeof EMPTY;

export function RegisterPlayerForm({ onRegistered }: { onRegistered: (registered: PlayerRegistered) => void }) {
  const t = useT();
  const [form, setForm] = useState<FormState>(EMPTY);
  const register = useRegisterPlayer((registered) => {
    setForm(EMPTY);
    onRegistered(registered);
  });
  const set = (field: keyof FormState) => (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setForm((f) => ({ ...f, [field]: e.target.value }));

  return (
    <ActionForm
      title={t("players.register")}
      submitLabel={t("players.register")}
      pending={register.pending}
      error={register.error}
      succeeded={register.succeeded}
      onSubmit={(reasonCode) =>
        register.run({
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          birthDate: form.birthDate,
          nationality: form.nationality.trim().toUpperCase(),
          documentType: form.documentType,
          documentNumber: form.documentNumber.trim(),
          phone: form.phone.trim() || null,
          reasonCode,
        })
      }
    >
      <TextField label={t("players.firstName")} value={form.firstName} onChange={set("firstName")} required autoComplete="off" />
      <TextField label={t("players.lastName")} value={form.lastName} onChange={set("lastName")} required autoComplete="off" />
      <TextField label={t("players.birthDate")} type="date" value={form.birthDate} onChange={set("birthDate")} required />
      <TextField
        label={t("players.nationality")}
        value={form.nationality}
        onChange={set("nationality")}
        required
        maxLength={2}
        autoComplete="off"
      />
      <SelectField label={t("players.documentType")} value={form.documentType} onChange={set("documentType")}>
        {DOCUMENT_TYPES.map((type) => (
          <option key={type} value={type}>
            {t(`players.documentType.${type}`)}
          </option>
        ))}
      </SelectField>
      <TextField
        label={t("players.documentNumber")}
        value={form.documentNumber}
        onChange={set("documentNumber")}
        required
        autoComplete="off"
      />
      <TextField label={t("players.phone")} type="tel" value={form.phone} onChange={set("phone")} autoComplete="off" />
    </ActionForm>
  );
}
