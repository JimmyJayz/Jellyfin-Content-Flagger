define(['page', 'events'], function(page, events) {
    return {
        load: function() {
            document.addEventListener('itemdetailspage', function() {
                const button = document.createElement('button');
                button.innerText = 'Flag Content';
                button.className = 'raised button';
                button.onclick = showFlagDialog;
                document.querySelector('.itemDetailsGroup')?.appendChild(button);
            });

            function showFlagDialog() {
                const dialog = document.createElement('div');
                dialog.style.position = 'fixed';
                dialog.style.background = '#333';
                dialog.style.color = '#fff';
                dialog.style.padding = '20px';
                dialog.style.zIndex = '1000';
                dialog.innerHTML = `
                    <h2>Flag Content</h2>
                    <select id="flagIssue">
                        <option value="Missing or Wrong CC">Missing or Wrong CC</option>
                        <option value="Bad Audio">Bad Audio</option>
                        <option value="Wrong Info">Wrong Info</option>
                    </select>
                    <textarea id="flagComment" placeholder="Optional comment" style="width:100%;margin-top:10px;"></textarea>
                    <button onclick="submitFlag()">Submit</button>
                    <button onclick="this.parentElement.remove()">Cancel</button>
                `;
                document.body.appendChild(dialog);
            }

            window.submitFlag = function() {
                const itemId = page.getCurrentItemId();
                const issue = document.getElementById('flagIssue').value;
                const comment = document.getElementById('flagComment').value;

                ApiClient.ajax({
                    url: ApiClient.getUrl('/ContentFlagger/Flag'),
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ itemId, issue, comment })
                }).then(() => {
                    alert('Content flagged successfully!');
                    dialog.remove();
                }).catch(err => {
                    alert('Error flagging content: ' + err);
                });
            };
        }
    };
});
