import { useEffect, useRef } from "react";

function Reveal({
    as: Tag = "div",
    className = "",
    delay = 0,
    children,
    ...rest
}) {
    const ref = useRef(null);

    useEffect(() => {
        const node = ref.current;
        if (!node) return;

        const motion = window.matchMedia("(prefers-reduced-motion: reduce)");
        if (motion.matches) {
            node.classList.add("is-visible");
            return;
        }

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        node.classList.add("is-visible");
                        observer.disconnect();
                    }
                });
            },
            { threshold: 0.14, rootMargin: "0px 0px -48px 0px" },
        );

        observer.observe(node);
        return () => observer.disconnect();
    }, []);

    return (
        <Tag
            ref={ref}
            className={`reveal ${className}`.trim()}
            style={delay ? { transitionDelay: `${delay}ms` } : undefined}
            {...rest}
        >
            {children}
        </Tag>
    );
}

export default Reveal;
