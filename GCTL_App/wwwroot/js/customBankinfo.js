    const addBtn = document.getElementById('add_bank_info');
    const modal = document.getElementById('modal');
    const closeModal = document.getElementById('closeModal');
    const saveBtn = document.getElementById('saveBtn');
    const infoTable = document.getElementById('infoTable').querySelector('tbody');

    let editIndex = null;

    // Open modal
    addBtn.addEventListener('click', () => {
      modal.style.display = 'flex';
      resetForm();
    });

    // Close modal
    closeModal.addEventListener('click', () => {
      modal.style.display = 'none';
    });

    // Save data
    saveBtn.addEventListener('click', () => {
      const bankName = document.getElementById('bankName').value;
      const branchName = document.getElementById('branchName').value;
      const accountName = document.getElementById('accountName').value;
      const accountNumber = document.getElementById('accountNumber').value;

      if (bankName && accountName && accountNumber) {
        if (editIndex === null) {
          const row = createRow(bankName, branchName, accountName, accountNumber);
          infoTable.appendChild(row);
        } else {
          updateRow(editIndex, bankName, branchName, accountName, accountNumber);
        }
        modal.style.display = 'none';
        resetForm();
      } else {
        alert('Please fill all fields');
      }
    });

    // Create a new row
    function createRow(bankName, branchName, accountName, accountNumber) {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${bankName}</td>
        <td>${branchName}</td>
        <td>${accountName}</td>
        <td>${accountNumber}</td>
        <td class="actions">
          <button class="edit"><span class="fas fa-edit"></span></button>
          <button class="delete"><span class="fas fa-trash"></span></button>
        </td>
      `;
      addRowListeners(row);
      return row;
    }

    // Add listeners to row buttons
    function addRowListeners(row) {
      const editBtn = row.querySelector('.edit');
      const deleteBtn = row.querySelector('.delete');

      editBtn.addEventListener('click', () => {
        editIndex = Array.from(infoTable.children).indexOf(row);
        const cells = row.children;
        document.getElementById('bankName').value = cells[0].textContent;
        document.getElementById('branchName').value = cells[1].textContent;
        document.getElementById('accountName').value = cells[2].textContent;
        document.getElementById('accountNumber').value = cells[3].textContent;
        modal.style.display = 'flex';
      });

      deleteBtn.addEventListener('click', () => {
        row.remove();
      });
    }

    // Update row data
    function updateRow(index, bankName, branchName, accountName, accountNumber) {
      const row = infoTable.children[index];
      row.children[0].textContent = bankName;
      row.children[1].textContent = branchName;
      row.children[2].textContent = accountName;
      row.children[3].textContent = accountNumber;
      editIndex = null;
    }

    // Reset form
    function resetForm() {
      document.getElementById('bankName').value = '';
      document.getElementById('branchName').value = '';
      document.getElementById('accountName').value = '';
      document.getElementById('accountNumber').value = '';
      editIndex = null;
    }


    const addBankDetailsBtn = document.getElementById('add_bank_detail');
    const bankDetailsModal = document.getElementById('bankDetailsModal');
    const closeBankDetailsModal = document.getElementById('closeBankDetailsModal');

      // Open modal
      addBankDetailsBtn.addEventListener('click', () => {
        bankDetailsModal.style.display = 'flex';
        resetForm();
      });
  
      // Close modal
      closeBankDetailsModal.addEventListener('click', () => {
        bankDetailsModal.style.display = 'none';
      });