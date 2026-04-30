import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";

export type Theme = "aurora" | "sunrise" | "ocean";

interface ThemeContextValue {
  theme: Theme;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: "ocean",
  setTheme: () => {},
});

function resolveInitialTheme(): Theme {
  const saved = localStorage.getItem("ui-theme");
  if (saved === "aurora" || saved === "sunrise" || saved === "ocean") return saved;
  return "ocean";
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(resolveInitialTheme);

  function setTheme(newTheme: Theme) {
    setThemeState(newTheme);
    localStorage.setItem("ui-theme", newTheme);
    document.documentElement.setAttribute("data-theme", newTheme);
  }

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", theme);
  }, [theme]);

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  return useContext(ThemeContext);
}
