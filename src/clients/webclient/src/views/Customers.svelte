<script>
    import { onDestroy, onMount } from "svelte";
    import Link from "../components/Link.svelte";
    import auth from "../services/authentication.js";
    import restService from "../services/restService.js";
    let plans = [];
    let newPlanName;

	let getCustomers = async () => {
        return await restService.getWithAuth("customers");
    };
    let promise = getCustomers();

    let detail = null;
    let getCustomerDetail = async (id) => {
        try {
            detail = await restService.getWithAuth(`customers/${id}`);
        } catch (ex) {
            if (ex.message) {
                detail = { exception: ex.message};
            } else
                detail = {"reason": "probably no rights"}
        }
    };
</script>
<p>This page tries to fetch customers from the web api, you should get "Forbidden" if not logged in and a list of customers if you log in</p>

<Link page="{{ path: '/register-customer', name: 'Register Customer' }}" />

{#await promise}
	<p>...waiting</p>
{:then customers}
    
    <ul>
	{#each customers as cus, i}
        <li on:click="{() => getCustomerDetail(cus.id)}">({cus.id}) {cus.lastName} - {cus.firstName}</li>
    {/each}
    </ul>

{:catch error}
	<p style="color: red">{error.message}</p>
{/await}

{#if detail}
    <p>Details</p>
    <p>{JSON.stringify(detail)}</p>
{/if}