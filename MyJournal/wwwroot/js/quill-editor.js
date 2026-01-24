let quill;

window.initQuillEditor = (editorId) => {
    quill = new Quill(`#${editorId}`, {
        theme: 'snow',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                ['blockquote', 'code-block'],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                [{ 'script': 'sub'}, { 'script': 'super' }],
                [{ 'indent': '-1'}, { 'indent': '+1' }],
                [{ 'color': [] }, { 'background': [] }],
                [{ 'align': [] }],
                ['clean'],
                ['link', 'image']
            ]
        },
        placeholder: 'Write your journal entry here...'
    });
};

window.getQuillHtml = (editorId) => {
    const editor = document.querySelector(`#${editorId}`);
    if (editor && editor.__quill) {
        return editor.__quill.root.innerHTML;
    }
    return quill ? quill.root.innerHTML : '';
};

window.setQuillHtml = (editorId, html) => {
    const editor = document.querySelector(`#${editorId}`);
    if (editor && editor.__quill) {
        editor.__quill.root.innerHTML = html;
    } else if (quill) {
        quill.root.innerHTML = html;
    }
};

window.clearQuillEditor = (editorId) => {
    const editor = document.querySelector(`#${editorId}`);
    if (editor && editor.__quill) {
        editor.__quill.setText('');
    } else if (quill) {
        quill.setText('');
    }
};
