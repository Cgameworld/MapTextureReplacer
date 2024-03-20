//from hookui-framework

const useDataUpdate = (react, event, onUpdate, deps) => {
    return react.useEffect(() => {
        const updateEvent = event + ".update"
        const subscribeEvent = event + ".subscribe"
        const unsubscribeEvent = event + ".unsubscribe"

        var sub = engine.on(updateEvent, (data) => {
            onUpdate && onUpdate(data)
        })

        engine.trigger(subscribeEvent)
        return () => {
            engine.trigger(unsubscribeEvent)
            sub.clear()
        };
    }, deps || [])
}

export default useDataUpdate