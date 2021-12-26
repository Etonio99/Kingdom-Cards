# Kingdom-Cards

This game was created in Unity 3D and programmed in C#. All of the assets were created by me.

Kingdom Cards is a card game where players verse each other 1v1 and must strategically play their cards in order to rid the opponent of their health points and come out a winner. This project has been an idea for several years and I hope to finish prodution sometime in the future.

The app can be downloaded in an open-testing state at https://play.google.com/store/apps/details?id=com.ebreiny.KingdomCards.

This app has log-in capabilites through Microsoft Azure Playfab. If you would like to play the game yourself without creating an account, you can log in with the following credentials:
Email: test@test.test
Password: Tester

## Gameplay

The game currently does not have in-game instructions. Each player has a deck of 20 cards. At the beginning of a match, each player draws 3 cards. These three cards are your hand. Each card has a mana cost (blue diamond), health points (red heart), and attack value (yellow sword). You can only play cards from your hand. To play a card you must have the same or more mana as the mana cost of that card. Both players play a single chosen card at the same time. This starts what is called a 'round'. Each card will deal their attack value to the other at the same time, removing health points from each other. The first card to run out of health points will be removed, allowing the surviving card to deal full attack value damage directly to the opponents health. If both cards tie, both are removed and the next round can start. To win, you must get your opponent's health points to 0.

Each player gains 2 mana each round. The amount you collect each round will increase to 3 after round 5. The maximum amount of mana you can have at once is 12, and any mana that goes over 12 is wasted.

There are 3 different types of cards; creature cards, spell cards, and structure cards. Creature cards are the most common and have more than 0 health points. Creature cards can have abilities, but not all of them do. Spell cards have 0 health but always have abilites. Structure cards are not currently implemented in this version, but are lingering cards that grant affects over a certain number of rounds. Structure cards also have 0 health points. Tap and hold on a card to view its abilities.

If a player has no cards left in their deck, they reshuffle their cards (not including their current hand) to allow them to draw more.

<img src='https://user-images.githubusercontent.com/65688007/147422079-e4df7565-afa1-4d25-92c4-26f9d28c20c9.png' width=24% height=24%> <img src='https://user-images.githubusercontent.com/65688007/147422081-52745f0a-54f5-47c1-a935-7b50eb2b0696.png' width=24% height=24%> <img src='https://user-images.githubusercontent.com/65688007/147422082-ab7071af-57d5-40c8-90bd-535584119a57.png' width=24% height=24%> <img src='https://user-images.githubusercontent.com/65688007/147422083-0fc9f6c9-6557-48d8-a12f-ff7b01c3406c.png' width=24% height=24%>

## Code
