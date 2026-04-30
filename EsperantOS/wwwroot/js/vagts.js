document.addEventListener('DOMContentLoaded', () => {
    // Lukkeknap
    const closeBtn = document.querySelector('.close-btn');
    if (closeBtn) {
        closeBtn.addEventListener('click', () => {
            document.querySelector('.vagtplan-modal').style.display = 'none';
        });
    }

    // Gennemgå data logic for date tabs og sidebar
    let urlParams = new URLSearchParams(window.location.search);
    let currentView = urlParams.get('view') || 'vagtplan'; // vagtplan or frigivede
    let selectedDate = document.querySelector('.date-tabs .tab.active')?.getAttribute('data-date') || '';

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

    const sidebarBtns = document.querySelectorAll('.sidebar-btn');
    sidebarBtns.forEach(btn => {
        if (btn.getAttribute('data-view') === currentView) {
            btn.classList.add('active');
        } else if (btn.hasAttribute('data-view')) {
            btn.classList.remove('active');
        }

        btn.addEventListener('click', (e) => {
            sidebarBtns.forEach(b => {
                if (b.hasAttribute('data-view')) b.classList.remove('active');
            });
            e.target.classList.add('active');
            currentView = e.target.getAttribute('data-view');

            const newUrl = new URL(window.location);
            newUrl.searchParams.set('view', currentView);
            window.history.pushState({}, '', newUrl);

            renderView();
        });
    });

    function renderView() {
        document.querySelectorAll('.shift-list').forEach(list => {
            let listDate = list.getAttribute('data-date-content');

            if (currentView === 'vagtplan') {
                list.style.display = (listDate === selectedDate) ? 'flex' : 'none';
                document.getElementById('dateTabs').style.display = 'flex';
            } else {
                list.style.display = 'flex';
                document.getElementById('dateTabs').style.display = 'none';
            }

            let hasVisibleCards = false;

            list.querySelectorAll('.shift-card').forEach(card => {
                let isFrigivet = card.getAttribute('data-frigivet') === 'true';

                if (currentView === 'frigivede') {
                    if (!isFrigivet) {
                        card.style.display = 'none';
                    } else {
                        card.style.display = 'block';
                        hasVisibleCards = true;
                    }

                    let dateLabel = card.querySelector('.shift-date-label');
                    if (dateLabel) dateLabel.style.display = 'block';
                } else {
                    card.style.display = 'block';
                    hasVisibleCards = true;

                    let dateLabel = card.querySelector('.shift-date-label');
                    if (dateLabel) dateLabel.style.display = 'none';
                }
            });

            if (currentView === 'frigivede' && !hasVisibleCards) {
                list.style.display = 'none';
            }
        });
    }

    // Initial render
    renderView();

    // Frigiv knapper
    document.querySelectorAll('.btn-release').forEach(btn => {
        if (btn.innerText === "Frigiv vagt") {
            btn.addEventListener('click', async (e) => {
                const button = e.target;
                const shiftId = button.getAttribute('data-id');
                if (!shiftId) return;

                try {
                    const response = await fetch(`/Vagts/Frigiv/${shiftId}`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' }
                    });

                    if (response.ok) {
                        window.location.reload();
                    } else {
                        alert('Kunne ikke frigive vagten. Prøv igen.');
                    }
                } catch (error) {
                    console.error('Fejl ved frigivelse af vagt:', error);
                }
            });
        }
    });

    // Tag vagt knapper
    document.querySelectorAll('.btn-take').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const button = e.target;
            const shiftId = button.getAttribute('data-id');
            if (!shiftId) return;

            try {
                const response = await fetch(`/Vagts/TagVagt/${shiftId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (response.ok) {
                    window.location.reload();
                } else {
                    alert('Kunne ikke tage vagten. Måske er den allerede taget?');
                }
            } catch (error) {
                console.error('Fejl ved at tage vagt:', error);
            }
        });
    });
});
