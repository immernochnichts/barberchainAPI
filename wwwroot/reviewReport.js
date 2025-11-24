window.guest = {
    getGuestId: function () {
        let id = localStorage.getItem("guestId");
        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem("guestId", id);
        }
        return id;
    },

    hasReported: function (reviewId) {
        const reported = JSON.parse(localStorage.getItem("reportedReviews") || "[]");
        return reported.includes(reviewId);
    },

    markReported: function (reviewId) {
        const reported = JSON.parse(localStorage.getItem("reportedReviews") || "[]");
        if (!reported.includes(reviewId)) {
            reported.push(reviewId);
            localStorage.setItem("reportedReviews", JSON.stringify(reported));
        }
    }
};
