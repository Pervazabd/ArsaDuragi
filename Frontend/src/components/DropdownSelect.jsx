import { useEffect, useId, useRef, useState } from "react";
import { Check, ChevronDown } from "lucide-react";

function DropdownSelect({
    ariaLabel,
    className = "",
    onChange,
    options,
    value,
    variant = "default",
}) {
    const [open, setOpen] = useState(false);
    const selectedIndex = Math.max(
        options.findIndex((option) => option.value === value),
        0,
    );
    const [activeIndex, setActiveIndex] = useState(selectedIndex);
    const id = useId();
    const rootRef = useRef(null);
    const triggerRef = useRef(null);
    const optionRefs = useRef([]);

    useEffect(() => {
        if (!open) {
            return undefined;
        }

        const handlePointerDown = (event) => {
            if (!rootRef.current?.contains(event.target)) {
                setOpen(false);
            }
        };
        const handleKeyDown = (event) => {
            if (event.key === "Escape") {
                setOpen(false);
                triggerRef.current?.focus();
            }
        };

        document.addEventListener("pointerdown", handlePointerDown);
        document.addEventListener("keydown", handleKeyDown);

        return () => {
            document.removeEventListener("pointerdown", handlePointerDown);
            document.removeEventListener("keydown", handleKeyDown);
        };
    }, [open]);

    useEffect(() => {
        if (open) {
            optionRefs.current[activeIndex]?.focus();
        }
    }, [open, activeIndex]);

    const openMenu = (index = selectedIndex) => {
        setActiveIndex(index);
        setOpen(true);
    };

    const moveActiveOption = (index) => {
        const nextIndex = (index + options.length) % options.length;
        setActiveIndex(nextIndex);
        optionRefs.current[nextIndex]?.focus();
    };

    const handleTriggerKeyDown = (event) => {
        if (event.key === "ArrowDown" || event.key === "ArrowUp") {
            event.preventDefault();
            openMenu(
                event.key === "ArrowDown"
                    ? selectedIndex
                    : Math.max(selectedIndex - 1, 0),
            );
        }
    };

    const handleOptionKeyDown = (event, index) => {
        if (event.key === "ArrowDown") {
            event.preventDefault();
            moveActiveOption(index + 1);
        } else if (event.key === "ArrowUp") {
            event.preventDefault();
            moveActiveOption(index - 1);
        } else if (event.key === "Home") {
            event.preventDefault();
            moveActiveOption(0);
        } else if (event.key === "End") {
            event.preventDefault();
            moveActiveOption(options.length - 1);
        } else if (event.key === "Escape") {
            event.preventDefault();
            setOpen(false);
            triggerRef.current?.focus();
        }
    };

    const selectedOption = options.find((option) => option.value === value);
    const compact = variant === "compact";
    const navbar = variant === "navbar";

    return (
        <div
            className={`relative ${compact ? "w-full sm:w-[245px]" : "w-full"} ${className}`}
            ref={rootRef}
        >
            <button
                ref={triggerRef}
                className={`group flex border-none bg-none w-full items-center justify-between gap-3 rounded-xl border text-left outline-none transition duration-200 focus-visible:ring-2 focus-visible:ring-ochre/40 ${navbar
                        ? "h-10  px-3 text-white hover:border-white/25 hover:bg-white/[0.08]"
                        : compact
                            ? "h-12  px-3.5"
                            : "mt-2 h-11  px-3 hover:border-ochre/60 hover:bg-[#dfdbd4]"
                    }`}
                type="button"
                aria-label={ariaLabel}
                aria-haspopup="listbox"
                aria-expanded={open}
                aria-controls={id}
                onClick={() =>
                    open ? setOpen(false) : openMenu(selectedIndex)
                }
                onKeyDown={handleTriggerKeyDown}
            >
                <span
                    className={`min-w-0 truncate ${navbar
                            ? "text-xs font-semibold tracking-wide text-white/85"
                            : compact
                                ? "text-sm font-medium text-ink"
                                : "text-sm text-ink"
                        }`}
                >
                    {selectedOption?.triggerLabel || selectedOption?.label}
                </span>
                <ChevronDown
                    size={compact ? 17 : 16}
                    className={`shrink-0 transition duration-300 ${navbar
                            ? "text-[#d7b783]"
                            : "text-muted group-hover:text-ochre"
                        } ${open ? "rotate-180 text-ochre" : ""} ${navbar && open ? "text-[#d7b783]" : ""
                        }`}
                />
            </button>

            <div
                className={`absolute top-[calc(100%+9px)] right-0 left-0 z-[70] origin-top overflow-hidden rounded-2xl border border-[#d9d4ca] bg-[#f7f5f0] p-1.5 shadow-[0_18px_50px_rgba(0,0,0,0.2)] ring-1 ring-black/[0.04] transition-[opacity,transform,visibility] duration-200 ease-out ${open
                        ? "visible translate-y-0 scale-100 opacity-100"
                        : "invisible -translate-y-1.5 scale-[0.98] opacity-0"
                    }`}
                id={id}
                role="listbox"
                aria-label={ariaLabel}
                aria-hidden={!open}
            >
                <span className="pointer-events-none absolute top-0 right-5 left-5 h-px bg-gradient-to-r from-transparent via-[#b99561]/60 to-transparent" />
                {options.map((option, index) => {
                    const selected = option.value === value;
                    const active = index === activeIndex;

                    return (
                        <button
                            ref={(element) => {
                                optionRefs.current[index] = element;
                            }}
                            className={`flex w-full items-center justify-between gap-3 rounded-xl px-3.5 py-3 text-left transition duration-150 ${selected
                                    ? "bg-[#e9e1d3] text-[#624b2d]"
                                    : active
                                        ? "bg-white text-ink shadow-sm"
                                        : "text-coal hover:bg-white/80"
                                }`}
                            type="button"
                            role="option"
                            aria-selected={selected}
                            tabIndex={active ? 0 : -1}
                            key={option.value}
                            onClick={() => {
                                onChange(option.value);
                                setOpen(false);
                                triggerRef.current?.focus();
                            }}
                            onFocus={() => setActiveIndex(index)}
                            onKeyDown={(event) =>
                                handleOptionKeyDown(event, index)
                            }
                        >
                            <span className="min-w-0">
                                <span className="block truncate text-[13px] font-medium">
                                    {option.label}
                                </span>
                                {option.description && (
                                    <span className="mt-0.5 block text-[10px] text-muted">
                                        {option.description}
                                    </span>
                                )}
                            </span>
                            <span
                                className={`grid size-6 shrink-0 place-items-center rounded-lg transition ${selected
                                        ? "bg-[#b99561]/15 text-[#9a794f]"
                                        : "text-transparent"
                                    }`}
                            >
                                <Check size={14} />
                            </span>
                        </button>
                    );
                })}
            </div>
        </div>
    );
}

export default DropdownSelect;
