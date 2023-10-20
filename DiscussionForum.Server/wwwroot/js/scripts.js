function adjustTextareaHeight(textarea) {
    textarea.style.height = 'auto';  // Reset height to auto to correctly calculate scrollHeight
    textarea.style.height = `${textarea.scrollHeight}px`;  // Set height to scrollHeight
}

const observer = new MutationObserver((mutationsList, _) => {
    for (const mutation of mutationsList) {
        for (let node of mutation.addedNodes) {
            if (node.nodeType === 1 && node.tagName === 'TEXTAREA') {
                // Check if the added node is a textarea
                adjustTextareaHeight(node);
            }
        }
    }
});

// Start observing the entire document for changes
observer.observe(document, { childList: true, subtree: true });
