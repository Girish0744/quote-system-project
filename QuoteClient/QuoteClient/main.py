import requests

API_URL = "http://localhost:5004/api"

def list_quotes():
    res = requests.get(f"{API_URL}/Quotes")
    if res.ok:
        quotes = res.json()
        if not quotes:
            print("No quotes found.")
        for q in quotes:
            print(f"\n[{q['id']}] \"{q['content']}\" — {q['author'] or 'Unknown'} ❤️ {q['likes']}")
    else:
        print("❌ Failed to fetch quotes.")

def add_quote():
    content = input("Enter quote content: ")
    author = input("Enter author (optional): ")

    payload = {
        "content": content,
        "author": author
    }

    res = requests.post(f"{API_URL}/Quotes", json=payload)
    if res.ok:
        print("✅ Quote added!")
    else:
        print("❌ Failed to add quote:", res.status_code)

def like_quote():
    quote_id = input("Enter quote ID to like: ")

    try:
        quote_id = int(quote_id)
    except ValueError:
        print("Invalid ID. Must be a number.")
        return

    res = requests.put(f"{API_URL}/Quotes/like/{quote_id}")
    if res.ok:
        print("❤️ Quote liked!")
    else:
        print(f"❌ Failed to like quote. Status: {res.status_code}")


def tag_quote():
    quote_id = input("Enter quote ID to tag: ")
    tag_name = input("Enter tag name: ").strip()

    try:
        quote_id = int(quote_id)
    except ValueError:
        print("Invalid ID.")
        return

    # Step 1: Create (or get) the tag
    res_tag = requests.post(f"{API_URL}/Tags", json={"name": tag_name})
    if not res_tag.ok:
        print("❌ Failed to create tag.")
        return

    tag = res_tag.json()
    tag_id = tag["id"]

    # Step 2: Assign tag to quote
    res_assign = requests.post(f"{API_URL}/Tags/assign", json={
        "quoteId": quote_id,
        "tagId": tag_id
    })

    if res_assign.ok:
        print(f"🏷️ Tag '{tag_name}' assigned to quote {quote_id}")
    else:
        print("❌ Failed to assign tag.")


def filter_by_tag():
    tag = input("Enter tag to filter by: ").strip().lower()

    res = requests.get(f"{API_URL}/Quotes/bytag/{tag}")
    if res.ok:
        quotes = res.json()
        if not quotes:
            print(f"No quotes found with tag '{tag}'.")
            return
        for q in quotes:
            print(f"\n[{q['id']}] \"{q['content']}\" — {q['author'] or 'Unknown'} ❤️ {q['likes']}")
    else:
        print("❌ Failed to fetch quotes by tag.")

def edit_quote():
    try:
        quote_id = int(input("Enter the ID of the quote to edit: "))
    except ValueError:
        print("❌ Invalid ID.")
        return

    new_content = input("Enter new content: ").strip()
    new_author = input("Enter new author: ").strip()

    payload = {
        "id": quote_id,
        "content": new_content,
        "author": new_author
    }

    res = requests.put(f"{API_URL}/Quotes/{quote_id}", json=payload)
    if res.status_code == 204:
        print("✏️ Quote updated successfully!")
    elif res.status_code == 404:
        print("❌ Quote not found.")
    else:
        print(f"❌ Failed to update. Status: {res.status_code}")

def get_random_quote():
    res = requests.get(f"{API_URL}/Quotes/random")
    if res.ok:
        q = res.json()
        print(f"\n🎲 Random Quote:")
        print(f"[{q['id']}] \"{q['content']}\" — {q['author'] or 'Unknown'} ❤️ {q['likes']}")
    else:
        print("❌ Could not fetch random quote.")

def get_most_liked_quotes():
    res = requests.get(f"{API_URL}/Quotes/mostliked")
    if res.ok:
        quotes = res.json()
        print("\n🔥 Most Liked Quotes:")
        for q in quotes:
            print(f"[{q['id']}] \"{q['content']}\" — {q['author'] or 'Unknown'} ❤️ {q['likes']}")
    else:
        print("❌ Failed to fetch most liked quotes.")


def main():
    while True:
        print("1. List all quotes")
        print("2. Add a new quote")
        print("3. Like a quote ❤️")
        print("4. Tag a quote 🏷️")
        print("5. Filter quotes by tag 🔍")
        print("6. Edit a quote ✏️")
        print("7. Get a random quote 🎲")
        print("8. Get most liked quotes 🔥")
        print("9. Exit")



        choice = input("Choose an option: ")

        if choice == "1":
            list_quotes()
        elif choice == "2":
            add_quote()
        elif choice == "3":
            like_quote()
        elif choice == "4":
            tag_quote()
        elif choice == "5":
            filter_by_tag()
        elif choice == "6":
            edit_quote()
        elif choice == "7":
            get_random_quote()
        elif choice == "8":
            get_most_liked_quotes()
        elif choice == "9":
            print("Goodbye 👋")
            break   
        else:
            print("Invalid option. Try again.") 

if __name__ == "__main__":
    main()


