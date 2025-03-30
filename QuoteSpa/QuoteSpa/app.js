const apiUrl = "http://localhost:5004/api"; // Use HTTP since your API runs on port 5004

let allTags = [];

// Load quotes and tags on page load
window.onload = () => {
    loadQuotes();
    loadTags();
};

function loadTags() {
    fetch(`${apiUrl}/Tags`)
        .then(res => res.json())
        .then(data => {
            allTags = data.map(tag => tag.name.toLowerCase());

            const dropdown = document.getElementById("tag-filter");
            dropdown.innerHTML = `<option value="">-- Select a Tag --</option>`; // Reset dropdown
            data.forEach(tag => {
                const opt = document.createElement("option");
                opt.value = tag.name;
                opt.textContent = tag.name;
                dropdown.appendChild(opt);
            });
        });
}

function loadQuotes() {
    fetch(`${apiUrl}/Quotes`)
        .then(res => res.json())
        .then(renderQuotes)
        .catch(err => console.error("Failed to load quotes:", err));
}


function renderQuotes(data) {
    const container = document.getElementById("quotes");
    container.innerHTML = "";

    data.forEach(quote => {
        const div = document.createElement("div");
        div.innerHTML = `
          <p>"${quote.content}" — <em>${quote.author || "Unknown"}</em> ❤️ <strong>${quote.likes}</strong></p>
          <button onclick="likeQuote(${quote.id})">Like</button>
            <button onclick="showEditFromElement(this)"
                    data-id="${quote.id}" 
                    data-content="${quote.content.replace(/"/g, '&quot;')}" 
                    data-author="${(quote.author || '').replace(/"/g, '&quot;')}">
              Edit
            </button>

          <input type="text" id="tag-input-${quote.id}" placeholder="Add a tag" oninput="showSuggestions(${quote.id})" autocomplete="off" />
          <div class="suggestions" id="suggestions-${quote.id}"></div>
          <button onclick="addTag(${quote.id})">Add Tag</button>
        `;


        container.appendChild(div);
    });
}

function addQuote() {
    const content = document.getElementById("quote-content").value;
    const author = document.getElementById("quote-author").value;

    const quote = { content, author };

    fetch(`${apiUrl}/Quotes`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(quote)
    })
        .then(res => {
            if (res.ok) {
                setTimeout(loadQuotes, 100);
                document.getElementById("quote-content").value = "";
                document.getElementById("quote-author").value = "";
            }
        });
}

function likeQuote(id) {
    fetch(`${apiUrl}/Quotes/like/${id}`, {
        method: "PUT"
    }).then(() => {
        const selectedTag = document.getElementById("tag-filter").value;
        if (selectedTag) {
            filterQuotes();
        } else {
            loadQuotes();
        }
    });

}

function addTag(quoteId) {
    const tagInput = document.getElementById(`tag-input-${quoteId}`);
    const tagName = tagInput.value.trim();
    if (!tagName) return;

    // Step 1: Create or get tag
    fetch(`${apiUrl}/Tags`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: tagName })
    })
    .then(res => {
        if (!res.ok) throw new Error("Failed to create/retrieve tag");
        return res.json();
    })
    .then(tag => {
        if (!tag.id) {
            console.error("🚨 Tag object missing 'id':", tag);
            throw new Error("Invalid tag object returned from API.");
        }

        console.log("✅ Assigning tag:", { quoteId, tagId: tag.id });

        // Step 2: Assign tag to quote
        return fetch(`${apiUrl}/Tags/assign`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                quoteId: Number(quoteId),
                tagId: Number(tag.id)
            })
        });
    })
    .then(res => {
        if (res.ok) {
            alert("Tag added!");
            tagInput.value = "";
            loadTags(); // 🔁 Reload tag dropdown so new tag appears!
        }
     else {
            console.error("❌ Failed to assign tag. API responded with:", res.status);
        }
    })
    .catch(err => {
        console.error("❌ Error assigning tag:", err);
    });
}


function filterQuotes() {
    const tag = document.getElementById("tag-filter").value;
    if (!tag) {
        loadQuotes();
        return;
    }

    fetch(`${apiUrl}/Quotes/bytag/${tag.toLowerCase()}`)
        .then(res => res.json())
        .then(renderQuotes);
}

function clearFilter() {
    document.getElementById("tag-filter").value = "";
    loadQuotes();
}

function showSuggestions(quoteId) {
    const input = document.getElementById(`tag-input-${quoteId}`);
    const suggestionsDiv = document.getElementById(`suggestions-${quoteId}`);

    const query = input.value.toLowerCase();
    if (!query) {
        suggestionsDiv.innerHTML = "";
        return;
    }

    const matches = allTags.filter(tag => tag.startsWith(query));
    suggestionsDiv.innerHTML = "";

    matches.forEach(tag => {
        const div = document.createElement("div");
        div.className = "suggestion-item";
        div.textContent = tag;
        div.onclick = () => {
            input.value = tag;
            suggestionsDiv.innerHTML = "";
        };
        suggestionsDiv.appendChild(div);
    });
}

function showEdit(id, currentContent, currentAuthor) {
    const container = document.getElementById("quotes");
    const quoteDiv = document.createElement("div");

    quoteDiv.innerHTML = `
        <input type="text" id="edit-content-${id}" value="${currentContent}" />
        <input type="text" id="edit-author-${id}" value="${currentAuthor}" placeholder="Author (optional)" />
        <button onclick="saveEdit(${id})">Save</button>
        <button onclick="loadQuotes()">Cancel</button>
    `;

    container.innerHTML = ""; // Clear the quote list temporarily
    container.appendChild(quoteDiv); // Show only the edit form
}

function showEditFromElement(button) {
    const id = button.getAttribute("data-id");
    const content = button.getAttribute("data-content");
    const author = button.getAttribute("data-author");

    showEdit(id, content, author);
}

function saveEdit(id) {
    const content = document.getElementById(`edit-content-${id}`).value.trim();
    const author = document.getElementById(`edit-author-${id}`).value.trim();

    fetch(`${apiUrl}/Quotes/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ id, content, author })
    })
        .then(res => {
            if (res.ok) {
                alert("✅ Quote updated!");
                loadQuotes();
            } else {
                alert("❌ Failed to update quote.");
            }
        })
        .catch(err => console.error("Error updating quote:", err));
}

