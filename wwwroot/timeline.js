window.timelineHelper = {
    snapIndex: function (timelineId, clientX, blockWidth) {
        const el = document.getElementById(timelineId);
        const block = el.querySelector(".time-block"); // first cell

        if (!block) return 0;

        // Position of the first block, NOT the container
        const rect = block.getBoundingClientRect();

        // Mouse offset relative to the FIRST block
        const relativeX = clientX - rect.left;

        const safeX = Math.max(0, relativeX);
        return Math.floor(safeX / blockWidth);
    }
};

window.timelineHover = {
    clear: function (count) {
        for (let i = 0; i < count; i++) {
            const el = document.getElementById(`hover-cell-${i}`);
            if (!el) continue;
            el.classList.remove("highlight-ok");
            el.classList.remove("highlight-bad");
        }
    },

    highlight: function (indexes, ok) {
        indexes.forEach(i => {
            const el = document.getElementById(`hover-cell-${i}`);
            if (!el) return;
            if (ok) el.classList.add("highlight-ok");
            else el.classList.add("highlight-bad");
        });
    }
};