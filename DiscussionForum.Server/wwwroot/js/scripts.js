function adjustTextareaHeight(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight + 2}px`;
}

const observer = new MutationObserver((mutationsList, _) => {
    for (const mutation of mutationsList) {
        for (let node of mutation.addedNodes) {
            if (node.nodeType === 1 && node.tagName === 'TEXTAREA') {
                adjustTextareaHeight(node);
            }
        }
    }
});

observer.observe(document, { childList: true, subtree: true });

let profileBtn = document.getElementById("profile-btn");
let profileMenu = document.getElementById("profile-menu");
document.addEventListener("click", (e) => {
    if (profileBtn &&
        e.target !== profileBtn && !profileBtn.contains(e.target) &&
        e.target !== profileMenu && !profileMenu.contains(e.target)) {
        profileBtn.checked = false;
    }
});
