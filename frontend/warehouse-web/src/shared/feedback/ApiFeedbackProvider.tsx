import { notification } from "antd";
import { createContext, useContext, type PropsWithChildren } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../errors/problemDetails";
import { hasServerFieldErrors } from "../errors/serverFieldErrors";

type ApiFeedback = {
  notifyError: (error: unknown, fallbackKey: string) => void;
};

const ApiFeedbackContext = createContext<ApiFeedback>({
  notifyError: () => undefined,
});

export function ApiFeedbackProvider({ children }: PropsWithChildren) {
  const { t } = useTranslation();
  const [api, contextHolder] = notification.useNotification();

  function notifyError(error: unknown, fallbackKey: string) {
    if (hasServerFieldErrors(error)) {
      return;
    }

    api.error({
      message: t("errors.requestFailed"),
      description: getErrorMessage(t, error, fallbackKey),
      placement: "topRight",
    });
  }

  return (
    <ApiFeedbackContext.Provider value={{ notifyError }}>
      {contextHolder}
      {children}
    </ApiFeedbackContext.Provider>
  );
}

export function useApiFeedback(): ApiFeedback {
  return useContext(ApiFeedbackContext);
}
