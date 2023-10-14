$(document).ready(function () {
    $('#costForm').on('submit', function (e) {
        e.preventDefault(); // Prevent the form from submitting traditionally

        // Make an AJAX POST request to your Razor Page handler
        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                $('#newest-city').text('To maintain your current standard of living in ' + response.selectedNewCity + ', you will need to earn:');
                $('#new-income').text('$' + response.newIncome);
                $('#cost-difference').text('The cost of living is ' + response.displayDifference + '% ' + response.higherOrLower + ' in ' + response.selectedNewCity + '.');

                // Scroll to the response section with a smooth animation
                const responseSection = document.getElementById("response-section");
                responseSection.scrollIntoView({ behavior: 'smooth' });
            },
            error: function () {
                // Handle errors if needed
            }
        });
    });
});