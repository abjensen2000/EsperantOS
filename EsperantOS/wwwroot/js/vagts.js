// vagts.js – al klientside-logik til vagtplanen
// Håndterer: dato-faner, visningsskift (vagtplan/frigivede), frigiv-knapper og tag-vagt-knapper
// Al kommunikation med serveren sker via fetch() uden at genindlæse siden.

document.addEventListener('DOMContentLoaded', () => {

    // ──────────────────────────────────────────────────
    // INITIALISERING
    // Læs hvilken visning og dato der er aktiv fra URL'en
    // ──────────────────────────────────────────────────

    const urlParams = new URLSearchParams(window.location.search);

    // Hent visningstype fra URL (?view=vagtplan eller ?view=frigivede)
    // Standard er 'vagtplan' hvis parameteren ikke er sat
    let currentView = urlParams.get('view') || 'vagtplan';

    // Hent den aktive dato fra den forudvalgte dato-fane
    let selectedDate = document.querySelector('#dateTabs .tab.active')?.getAttribute('data-date') || '';

    // ──────────────────────────────────────────────────
    // DATO-FANER
    // Klik på en dato-fane skifter hvilke vagter der vises
    // ──────────────────────────────────────────────────

    const dateTabsContainer = document.getElementById('dateTabs');
    if (dateTabsContainer) {
        dateTabsContainer.addEventListener('click', (e) => {
            if (e.target.classList.contains('tab')) {
                // Fjern aktiv markering fra alle faner
                dateTabsContainer.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));

                // Markér den klikkede fane som aktiv og gem den valgte dato
                e.target.classList.add('active');
                selectedDate = e.target.getAttribute('data-date');

                // Opdatér hvad der vises på siden
                renderView();
            }
        });
    }

    // ──────────────────────────────────────────────────
    // VISNINGSSKIFT (sidebar-knapper)
    // Skifter mellem "Vagtplan" og "Frigivede vagter"
    // ──────────────────────────────────────────────────

    // Find alle sidebar-knapper der har en data-view attribut
    const navBtns = document.querySelectorAll('.nav-btn[data-view]');
    navBtns.forEach(btn => {
        // Sæt den rigtige knap som aktiv baseret på den nuværende URL-parameter
        if (btn.getAttribute('data-view') === currentView) btn.classList.add('active');
        else btn.classList.remove('active');

        btn.addEventListener('click', (e) => {
            // Fjern aktiv markering fra alle visningsknapper
            navBtns.forEach(b => b.classList.remove('active'));
            e.target.classList.add('active');

            // Opdatér den aktive visning
            currentView = e.target.getAttribute('data-view');

            // Opdatér URL'en uden at genindlæse siden (så browseren husker visningen)
            const newUrl = new URL(window.location);
            newUrl.searchParams.set('view', currentView);
            window.history.pushState({}, '', newUrl);

            renderView();
        });
    });

    // ──────────────────────────────────────────────────
    // RENDERFUNKTION
    // Bestemmer hvilke vagtlister og kort der er synlige
    // baseret på currentView og selectedDate
    // ──────────────────────────────────────────────────

    function renderView() {
        const dateTabs = document.getElementById('dateTabs');

        // Gennemgå alle dato-grupper af vagter
        document.querySelectorAll('.shift-list').forEach(list => {
            const listDate = list.getAttribute('data-date-content');

            if (currentView === 'vagtplan') {
                // Vis kun listen der matcher den valgte dato-fane
                list.style.display = (listDate === selectedDate) ? 'flex' : 'none';
                // Vis dato-fanerne i vagtplan-visning
                if (dateTabs) dateTabs.style.display = 'flex';
            } else {
                // I "frigivede"-visning vises alle lister (vi filtrerer på kortniveau nedenfor)
                list.style.display = 'flex';
                // Skjul dato-fanerne i frigivede-visning – de giver ikke mening her
                if (dateTabs) dateTabs.style.display = 'none';
            }

            let hasVisible = false; // Bruges til at skjule tomme dato-sektioner

            // Gennemgå alle vagtort i listen
            list.querySelectorAll('.shift-card').forEach(card => {
                const isFrigivet = card.getAttribute('data-frigivet') === 'true';
                const dateLabel = card.querySelector('.shift-date-label'); // Datoetiket på kortet

                if (currentView === 'frigivede') {
                    // Vis kun frigivede vagter – skjul resten
                    card.style.display = isFrigivet ? 'flex' : 'none';
                    if (isFrigivet) hasVisible = true;

                    // Vis dato-etiketten på frigivede kort (da fanerne er skjult)
                    if (dateLabel) dateLabel.style.display = isFrigivet ? 'block' : 'none';
                } else {
                    // Vis alle vagter i vagtplan-visning
                    card.style.display = 'flex';
                    hasVisible = true;

                    // Skjul dato-etiketten i vagtplan-visning (dato ses i fanerne)
                    if (dateLabel) dateLabel.style.display = 'none';
                }
            });

            // Skjul hele dato-sektionen hvis den ikke har nogen synlige frigivede vagter
            if (currentView === 'frigivede' && !hasVisible) list.style.display = 'none';
        });
    }

    // Kør renderView ved sideindlæsning for at sikre korrekt starttilstand
    renderView();

    // ──────────────────────────────────────────────────
    // FRIGIV VAGT
    // Sender POST-request til /Vagts/Frigiv/{id} og genindlæser siden ved succes
    // ──────────────────────────────────────────────────

    document.querySelectorAll('.btn-release').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const shiftId = e.target.getAttribute('data-id'); // Hent vagt-ID fra knappen
            if (!shiftId) return;

            try {
                const res = await fetch(`/Vagts/Frigiv/${shiftId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (res.ok) {
                    window.location.reload(); // Genindlæs siden så vagten vises som frigivet
                } else {
                    alert('Kunne ikke frigive vagten. Prøv igen.');
                }
            } catch (err) {
                console.error('Fejl ved frigivelse:', err);
            }
        });
    });

    // ──────────────────────────────────────────────────
    // TAG VAGT
    // Sender POST-request til /Vagts/TagVagt/{id} og genindlæser siden ved succes
    // ──────────────────────────────────────────────────

    document.querySelectorAll('.btn-take').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const shiftId = e.target.getAttribute('data-id'); // Hent vagt-ID fra knappen
            if (!shiftId) return;

            try {
                const res = await fetch(`/Vagts/TagVagt/${shiftId}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (res.ok) {
                    window.location.reload(); // Genindlæs siden så vagten vises som taget
                } else {
                    alert('Kunne ikke tage vagten. Måske er den allerede taget?');
                }
            } catch (err) {
                console.error('Fejl ved at tage vagt:', err);
            }
        });
    });

}); // Slut på DOMContentLoaded
