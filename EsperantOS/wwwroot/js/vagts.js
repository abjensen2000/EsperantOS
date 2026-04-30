document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    let currentView = urlParams.get('view') || 'vagtplan';
    let selectedDate = document.querySelector('#dateTabs .tab.active')?.getAttribute('data-date') || '';

    // Date tab clicks
    const dateTabsContainer = document.getElementById('dateTabs');
    if (dateTabsContainer) {
        dateTabsContainer.addEventListener('click', (e) => {
            if (e.target.classList.contains('tab')) {
                dateTabsContainer.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
                e.target.classList.add('active');
                selectedDate = e.target.getAttribute('data-date');
                renderView();
            }
        });
    }

    // Sidebar nav buttons (data-view switches)
    const navBtns = document.querySelectorAll('.nav-btn[data-view]');
    navBtns.forEach(btn => {
        if (btn.getAttribute('data-view') === currentView) btn.classList.add('active');
        else btn.classList.remove('active');

        btn.addEventListener('click', (e) => {
            navBtns.forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');
            currentView = e.target.getAttribute('data-view');

            const newUrl = new URL(window.location);
            newUrl.searchParams.set('view', currentView);
            window.history.pushState({}, '', newUrl);

            renderView();
        });
    });

    function renderView() {
        const dateTabs = document.getElementById('dateTabs');

        document.querySelectorAll('.shift-list').forEach(list => {
            const listDate = list.getAttribute('data-date-content');

            if (currentView === 'vagtplan') {
                list.style.display = (listDate === selectedDate) ? 'flex' : 'none';
                if (dateTabs) dateTabs.style.display = 'flex';
            } else {
                list.style.display = 'flex';
                if (dateTabs) dateTabs.style.display = 'none';
            }

            let hasVisible = false;
            list.querySelectorAll('.shift-card').forEach(card => {
                const isFrigivet = card.getAttribute('data-frigivet') === 'true';
                const dateLabel = card.querySelector('.shift-date-label');

                if (currentView === 'frigivede') {
                    card.style.display = isFrigivet ? 'flex' : 'none';
                    if (isFrigivet) hasVisible = true;
                    if (dateLabel) dateLabel.style.display = isFrigivet ? 'block' : 'none';
                } else {
                    card.style.display = 'flex';
                    hasVisible = true;
                    if (dateLabel) dateLabel.style.display = 'none';
                }
            });

            if (currentView === 'frigivede' && !hasVisible) list.style.display = 'none';
        });
    }

    renderView();

    // Frigiv buttons
    document.querySelectorAll('.btn-release').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const shiftId = e.target.getAttribute('data-id');
            if (!shiftId) return;
            try {
                const res = await fetch(`/Vagts/Frigiv/${shiftId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                if (res.ok) window.location.reload();
                else alert('Kunne ikke frigive vagten. Prøv igen.');
            } catch (err) {
                console.error(err);
            }
        });
    });

    // Tag vagt buttons
    document.querySelectorAll('.btn-take').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const shiftId = e.target.getAttribute('data-id');
            if (!shiftId) return;
            try {
                const res = await fetch(`/Vagts/TagVagt/${shiftId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });
                if (res.ok) window.location.reload();
                else alert('Kunne ikke tage vagten. Måske er den allerede taget?');
            } catch (err) {
                console.error(err);
            }
        });
    });
});
