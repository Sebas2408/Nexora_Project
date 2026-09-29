document.querySelectorAll("[data-product-image-mode]").forEach((selector) => {
    const form = selector.closest("form");
    if (!form) return;

    const panels = form.querySelectorAll("[data-product-image-panel]");

    const updatePanels = () => {
        panels.forEach((panel) => {
            const visible = panel.dataset.productImagePanel === selector.value;
            panel.hidden = !visible;
            panel.querySelectorAll("input").forEach((input) => {
                input.disabled = !visible;
            });
        });
    };

    selector.addEventListener("change", updatePanels);
    updatePanels();
});
