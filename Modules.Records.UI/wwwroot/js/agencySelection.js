window.authorityRecordsAgency = {
    setActiveAgency: function (cookieName, agencyId) {
        document.cookie = cookieName + "=" + encodeURIComponent(agencyId) + "; path=/; samesite=lax";
    }
};
